package com.littleweeps.family;

import android.content.Context;
import android.security.keystore.KeyGenParameterSpec;
import android.security.keystore.KeyProperties;
import android.util.AtomicFile;
import org.json.JSONObject;
import org.json.JSONArray;
import java.io.File;
import java.io.FileOutputStream;
import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.security.KeyStore;
import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.GCMParameterSpec;

/** A create-only player vault. Corruption/key loss requests parent help, never resets identity. */
public final class FamilyEnrollment {
    private static final String ALIAS="com.littleweeps.family.enrollment.v1";
    private static final byte[] AAD=ALIAS.getBytes(StandardCharsets.UTF_8);
    private static final int LIMIT=32768;
    private final Context context;
    private final AtomicFile vault;
    private String status="unpaired";
    public FamilyEnrollment(Context context) {
        this.context=context.getApplicationContext();
        vault=new AtomicFile(new File(this.context.getNoBackupFilesDir(),"family-enrollment.v1"));
    }
    public String status() { return status; }
    // Development provisioning is through the trusted USB/ADB channel, not an
    // exported Activity, intent, or secret embedded in the APK. Consume only on success.
    public synchronized String load() {
        try {
            File external=context.getExternalFilesDir(null);
            File inbox=external==null?null:new File(external,"FamilyLAN/enrollment.json");
            if(inbox!=null && inbox.isFile()) {
                if(inbox.length()>LIMIT)throw new IllegalArgumentException();
                String json=new String(Files.readAllBytes(inbox.toPath()),StandardCharsets.UTF_8);
                int added=add(json);
                if(added!=0){status="enrollment-needs-parent";return null;}
                if(!inbox.delete()){status="enrollment-needs-parent";return null;}
            }
            String json=read();status=json==null?"unpaired":"paired";return json;
        } catch(Exception e) { status="enrollment-needs-parent";return null; }
    }
    private SecretKey key(boolean create) throws Exception {
        KeyStore store=KeyStore.getInstance("AndroidKeyStore");store.load(null);
        if(store.containsAlias(ALIAS))return (SecretKey)store.getKey(ALIAS,null);
        if(!create)throw new IllegalStateException("Enrollment key unavailable.");
        KeyGenerator generator=KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES,"AndroidKeyStore");
        generator.init(new KeyGenParameterSpec.Builder(ALIAS,KeyProperties.PURPOSE_ENCRYPT|KeyProperties.PURPOSE_DECRYPT)
            .setBlockModes(KeyProperties.BLOCK_MODE_GCM).setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
            .setKeySize(256).setRandomizedEncryptionRequired(true).build());
        return generator.generateKey();
    }
    public synchronized String read() throws Exception {
        // AtomicFile also recovers a completed old .bak after interrupted writes.
        if(!vault.getBaseFile().exists() && !new File(vault.getBaseFile()+".bak").exists())return null;
        if(vault.getBaseFile().length()>LIMIT+128)throw new IllegalArgumentException();
        byte[] bytes=vault.readFully();
        if(bytes.length<33 || bytes.length>LIMIT+128 || bytes[0]!=1)throw new IllegalArgumentException();
        byte[] iv=new byte[12];System.arraycopy(bytes,1,iv,0,12);
        Cipher cipher=Cipher.getInstance("AES/GCM/NoPadding");
        cipher.init(Cipher.DECRYPT_MODE,key(false),new GCMParameterSpec(128,iv));cipher.updateAAD(AAD);
        String json=new String(cipher.doFinal(bytes,13,bytes.length-13),StandardCharsets.UTF_8);validate(json);return json;
    }
    public synchronized int add(String json) {
        try {
            validate(json);
            String existing=read();
            if(existing!=null)return existing.equals(json)?0:2;
            Cipher cipher=Cipher.getInstance("AES/GCM/NoPadding");cipher.init(Cipher.ENCRYPT_MODE,key(true));cipher.updateAAD(AAD);
            byte[] iv=cipher.getIV();if(iv.length!=12)throw new IllegalStateException();
            byte[] encrypted=cipher.doFinal(json.getBytes(StandardCharsets.UTF_8));
            byte[] blob=ByteBuffer.allocate(1+iv.length+encrypted.length).put((byte)1).put(iv).put(encrypted).array();
            FileOutputStream output=null;
            try { output=vault.startWrite();output.write(blob);vault.finishWrite(output); }
            catch(Exception e) { if(output!=null)vault.failWrite(output);throw e; }
            return json.equals(read())?0:3;
        } catch(Exception e) { return 1; }
    }
    private static String required(JSONObject value,String name) throws Exception {
        Object item=value.get(name);if(!(item instanceof String))throw new IllegalArgumentException();return (String)item;
    }
    private static boolean id(String value) { return value.matches("[0-9a-f]{32}"); }
    public static void validate(String json) throws Exception {
        if(json==null || json.getBytes(StandardCharsets.UTF_8).length>LIMIT)throw new IllegalArgumentException();
        JSONObject value=new JSONObject(json);
        Object schema=value.get("schema");
        if(!(schema instanceof Integer) || ((Integer)schema)!=1 || !"client".equals(required(value,"role"))
            || !id(required(value,"familyId")) || !id(required(value,"authorityId")) || !id(required(value,"worldId")) || !id(required(value,"profile"))
            || !required(value,"credential").matches("[0-9a-f]{64}") || !required(value,"serverName").equals("lw-"+required(value,"authorityId")+".local"))throw new IllegalArgumentException();
        String ca=required(value,"caCertificate");if(ca.isEmpty() || ca.length()>8192)throw new IllegalArgumentException();
        for(String field:new String[]{"privateKey","certificate"})if(!value.isNull(field) && !"".equals(value.get(field)))throw new IllegalArgumentException();
        if(!value.isNull("members")) { Object members=value.get("members");if(!(members instanceof JSONArray) || ((JSONArray)members).length()!=0)throw new IllegalArgumentException(); }
    }
}
