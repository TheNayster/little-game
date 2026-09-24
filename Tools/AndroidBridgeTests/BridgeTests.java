package com.littleweeps.family.tests;

import android.app.Instrumentation;
import android.content.Context;
import android.net.nsd.NsdManager;
import android.net.nsd.NsdServiceInfo;
import android.os.Bundle;
import com.littleweeps.family.FamilyDiscovery;
import com.littleweeps.family.FamilyEnrollment;
import org.json.JSONObject;
import org.json.JSONArray;
import java.io.File;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.UUID;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

/** Isolated native test package; never included in the family game APK. */
public final class BridgeTests extends Instrumentation {
    private Bundle args;
    private final JSONArray passed=new JSONArray();
    public void onCreate(Bundle args){this.args=args;start();}
    private void check(boolean condition,String name) throws Exception {
        if(!condition)throw new IllegalStateException(name);passed.put(name);
    }
    public void onStart(){
        Bundle result=new Bundle();
        try {
            Context context=getTargetContext();
            if("resume".equals(args.getString("phase")))resume(context);
            else if("pc".equals(args.getString("phase")))pc(context);
            else test(context);
            result.putString("result",new JSONObject().put("passed",passed).put("count",passed.length()).toString());finish(-1,result);
        }catch(Exception e){result.putString("result",new JSONObject().toString());result.putString("failure",e.getClass().getSimpleName()+": "+e.getMessage());finish(1,result);}
    }
    private void pc(Context context) throws Exception {
        try(FamilyDiscovery discovery=new FamilyDiscovery(context,args.getString("family"),args.getString("authority"),args.getString("world"),3,3)) {
            discovery.start();long deadline=System.currentTimeMillis()+15000;String answer=null;
            while(System.currentTimeMillis()<deadline && answer==null){answer=discovery.take();Thread.sleep(100);}
            check(answer!=null && new JSONObject(answer).getInt("port")==Integer.parseInt(args.getString("port")) && discovery.error()==0,"Actual Windows DNS-SD advertisement resolves through the Android adapter");
        }
    }
    private void resume(Context context) throws Exception {
        String fixture=new String(Files.readAllBytes(new File(context.getFilesDir(),"fixture.json").toPath()),StandardCharsets.UTF_8);
        FamilyEnrollment vault=new FamilyEnrollment(context);
        check(fixture.equals(vault.load()) && "paired".equals(vault.status()),"Keystore enrollment survives a new process");
    }
    private void test(Context context) throws Exception {
        File fixtureFile=new File(context.getFilesDir(),"fixture.json");
        JSONObject fixture;
        if(fixtureFile.exists())fixture=new JSONObject(new String(Files.readAllBytes(fixtureFile.toPath()),StandardCharsets.UTF_8));
        else {
            String family=UUID.randomUUID().toString().replace("-",""), authority=UUID.randomUUID().toString().replace("-",""),world=UUID.randomUUID().toString().replace("-","");
            fixture=new JSONObject().put("schema",1).put("role","client").put("familyId",family).put("authorityId",authority).put("worldId",world)
                .put("profile",UUID.randomUUID().toString().replace("-","")).put("credential",UUID.randomUUID().toString().replace("-","")+UUID.randomUUID().toString().replace("-",""))
                .put("serverName","lw-"+authority+".local").put("caCertificate","isolated vault test; no network trust");
            Files.write(fixtureFile.toPath(),fixture.toString().getBytes(StandardCharsets.UTF_8));
        }
        String json=fixture.toString();FamilyEnrollment vault=new FamilyEnrollment(context);
        check(vault.add(json)==0 && json.equals(vault.read()),"AES-GCM Keystore enrollment round trip");
        check(vault.add(json)==0,"Identical enrollment retry is idempotent");
        check(vault.add(new JSONObject(json).put("profile",UUID.randomUUID().toString().replace("-","")).toString())!=0 && json.equals(vault.read()),"Replacement player is refused without changing saved identity");
        for(String field:new String[]{"privateKey","certificate","members","schema","credential","role"}) {
            Object bad=field.equals("members")?new JSONArray().put("other-player"):field.equals("schema")?"1":field.equals("credential")?true:"server secret";
            check(vault.add(new JSONObject(json).put(field,bad).toString())!=0 && json.equals(vault.read()),"Reject malformed/server material: "+field);
        }
        File data=new File(context.getNoBackupFilesDir(),"family-enrollment.v1");byte[] encrypted=Files.readAllBytes(data.toPath());
        check(!new String(encrypted,StandardCharsets.ISO_8859_1).contains(fixture.getString("credential")),"Private no-backup file contains ciphertext, not the player credential");
        byte[] damaged=encrypted.clone();damaged[damaged.length-1]^=1;Files.write(data.toPath(),damaged);
        try { check(vault.load()==null && "enrollment-needs-parent".equals(vault.status()) && vault.add(json)!=0,"Tampering fails closed and does not replace the vault"); }
        finally { Files.write(data.toPath(),encrypted); }
        File inbox=new File(context.getExternalFilesDir(null),"FamilyLAN/enrollment.json");inbox.getParentFile().mkdirs();Files.write(inbox.toPath(),json.getBytes(StandardCharsets.UTF_8));
        check(json.equals(vault.load()) && !inbox.exists(),"Validated USB inbox is removed after protected persistence");
        File wrongInbox=inbox;Files.write(wrongInbox.toPath(),new JSONObject(json).put("role","server").toString().getBytes(StandardCharsets.UTF_8));
        try { check(vault.load()==null && wrongInbox.exists() && json.equals(vault.read()),"Bad inbox is preserved for parent review; existing identity is intact"); }
        finally { wrongInbox.delete(); }
        discovery(context,fixture);
    }
    private void discovery(Context context,JSONObject fixture) throws Exception {
        NsdManager manager=(NsdManager)context.getSystemService(Context.NSD_SERVICE);
        String family=fixture.getString("familyId"),authority=fixture.getString("authorityId"),world=fixture.getString("worldId");
        NsdServiceInfo service=new NsdServiceInfo();service.setServiceName("LW-"+authority);service.setServiceType("_lw-playset._udp");service.setPort(42671);
        service.setAttribute("v","1");service.setAttribute("family",family);service.setAttribute("authority",authority);service.setAttribute("world",world);service.setAttribute("protocol","3");service.setAttribute("content","3");
        CountDownLatch registered=new CountDownLatch(1),stopped=new CountDownLatch(1);final boolean[] okay={false};
        NsdManager.RegistrationListener listener=new NsdManager.RegistrationListener(){
            public void onServiceRegistered(NsdServiceInfo info){okay[0]=info.getServiceName().equals("LW-"+authority);registered.countDown();}
            public void onRegistrationFailed(NsdServiceInfo info,int code){registered.countDown();}
            public void onServiceUnregistered(NsdServiceInfo info){stopped.countDown();}
            public void onUnregistrationFailed(NsdServiceInfo info,int code){stopped.countDown();}
        };
        manager.registerService(service,NsdManager.PROTOCOL_DNS_SD,listener);
        try {
            check(registered.await(15,TimeUnit.SECONDS) && okay[0],"Native NSD test service registered");
            for(int cycle=0;cycle<3;cycle++)try(FamilyDiscovery discovery=new FamilyDiscovery(context,family,authority,world,3,3)) {
                discovery.start();long deadline=System.currentTimeMillis()+15000;String answer=null;
                while(System.currentTimeMillis()<deadline && answer==null){answer=discovery.take();Thread.sleep(100);}
                check(answer!=null && new JSONObject(answer).getInt("port")==42671 && discovery.error()==0,"Native browse/resolve cycle "+(cycle+1));
                discovery.close();Thread.sleep(250);check(discovery.take()==null,"Closed browser drops late results "+(cycle+1));
            }
            try(FamilyDiscovery wrong=new FamilyDiscovery(context,UUID.randomUUID().toString().replace("-",""),authority,world,3,3)) {
                wrong.start();Thread.sleep(2500);check(wrong.take()==null && wrong.error()==0,"Different-family TXT cannot produce an endpoint");
            }
            try(FamilyDiscovery wrong=new FamilyDiscovery(context,family,authority,world,99,3)) {
                wrong.start();Thread.sleep(2500);check(wrong.take()==null && wrong.error()==0,"Protocol mismatch cannot produce an endpoint");
            }
        } finally { manager.unregisterService(listener);stopped.await(5,TimeUnit.SECONDS); }
    }
}
