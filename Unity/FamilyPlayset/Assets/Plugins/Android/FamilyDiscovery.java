package com.littleweeps.family;

import android.content.Context;
import android.net.ConnectivityManager;
import android.net.LinkProperties;
import android.net.nsd.NsdManager;
import android.net.nsd.NsdServiceInfo;
import android.os.Build;
import org.json.JSONObject;
import java.net.Inet4Address;
import java.net.InetAddress;
import java.net.NetworkInterface;
import java.nio.charset.StandardCharsets;
import java.util.ArrayDeque;
import java.util.Collections;
import java.util.Map;

/** Native discovery supplies hints only. Unity still validates enrolled DTLS trust. */
public final class FamilyDiscovery implements AutoCloseable {
    private static final String TYPE = "_lw-playset._udp";
    private final NsdManager manager;
    private final ConnectivityManager connectivity;
    private final String family, authority, world;
    private final int protocol, content;
    private final ArrayDeque<String> answers = new ArrayDeque<>();
    private boolean closed, browsing, resolving;
    private int error;
    private NsdManager.DiscoveryListener browse;
    private NsdManager.ResolveListener legacy;
    private Modern modern;

    public FamilyDiscovery(Context context, String family, String authority, String world, int protocol, int content) {
        if (!id(family) || !id(authority) || !id(world)) throw new IllegalArgumentException("Invalid discovery identity.");
        this.family=family; this.authority=authority; this.world=world; this.protocol=protocol; this.content=content;
        manager=(NsdManager)context.getSystemService(Context.NSD_SERVICE);
        connectivity=(ConnectivityManager)context.getSystemService(Context.CONNECTIVITY_SERVICE);
    }
    private static boolean id(String value) { return value!=null && value.matches("[0-9a-f]{32}"); }
    private boolean matches(NsdServiceInfo info) {
        return info!=null && (TYPE.equals(info.getServiceType()) || (TYPE+".").equals(info.getServiceType()))
            && ("LW-"+authority).equals(info.getServiceName());
    }
    public synchronized void start() {
        if (closed || browsing) throw new IllegalStateException("Discovery already started or closed.");
        browsing=true;
        browse=new NsdManager.DiscoveryListener() {
            public void onDiscoveryStarted(String type) { }
            public void onDiscoveryStopped(String type) { }
            public void onStartDiscoveryFailed(String type,int code) { synchronized(FamilyDiscovery.this) { if(!closed)error=code==0?-1:code; } }
            public void onStopDiscoveryFailed(String type,int code) { synchronized(FamilyDiscovery.this) { if(!closed)error=code==0?-1:code; } }
            public void onServiceLost(NsdServiceInfo info) { synchronized(FamilyDiscovery.this) { if(!closed && matches(info))answers.clear(); } }
            public void onServiceFound(NsdServiceInfo info) { resolve(info); }
        };
        try { manager.discoverServices(TYPE,NsdManager.PROTOCOL_DNS_SD,browse); }
        catch (RuntimeException e) { error=-1; }
    }
    @SuppressWarnings("deprecation")
    private synchronized void resolve(NsdServiceInfo info) {
        if(closed || resolving || !matches(info))return;
        resolving=true;
        try {
            // The current Samsung uses this cancelable API. Keep the older API
            // isolated for minSdk 26; its late callbacks must never revive a closed browser.
            if(Build.VERSION.SDK_INT>=34) { modern=new Modern(info); return; }
            legacy=new NsdManager.ResolveListener() {
                public void onResolveFailed(NsdServiceInfo service,int code) { synchronized(FamilyDiscovery.this) { resolving=false; } }
                public void onServiceResolved(NsdServiceInfo service) { synchronized(FamilyDiscovery.this) { resolving=false; accept(service); } }
            };
            manager.resolveService(info,legacy);
        } catch(RuntimeException e) { resolving=false; error=-2; }
    }
    private final class Modern implements NsdManager.ServiceInfoCallback {
        Modern(NsdServiceInfo info) { manager.registerServiceInfoCallback(info,Runnable::run,this); }
        public void onServiceInfoCallbackRegistrationFailed(int code) { synchronized(FamilyDiscovery.this) { if(!closed){error=code==0?-2:code;resolving=false;modern=null;} } }
        public void onServiceUpdated(NsdServiceInfo info) { synchronized(FamilyDiscovery.this) { accept(info); } }
        public void onServiceLost() { synchronized(FamilyDiscovery.this) { answers.clear(); } }
        public void onServiceInfoCallbackUnregistered() { }
    }
    private static String text(Map<String,byte[]> attributes,String key) {
        byte[] bytes=attributes.get(key);
        return bytes==null || bytes.length>128 ? "" : new String(bytes,StandardCharsets.UTF_8);
    }
    @SuppressWarnings("deprecation")
    private synchronized void accept(NsdServiceInfo info) {
        if(closed || !matches(info) || info.getPort()<1024 || info.getPort()>65535)return;
        Map<String,byte[]> txt=info.getAttributes();
        if(!"1".equals(text(txt,"v")) || !family.equals(text(txt,"family")) || !authority.equals(text(txt,"authority")) || !world.equals(text(txt,"world"))
            || !Integer.toString(protocol).equals(text(txt,"protocol")) || !Integer.toString(content).equals(text(txt,"content")))return;
        try {
            int index=0;
            if(Build.VERSION.SDK_INT>=33 && info.getNetwork()!=null) {
                LinkProperties link=connectivity.getLinkProperties(info.getNetwork());
                NetworkInterface nic=link==null?null:NetworkInterface.getByName(link.getInterfaceName());
                if(nic!=null)index=nic.getIndex();
            }
            Iterable<InetAddress> hosts=Build.VERSION.SDK_INT>=34 ? info.getHostAddresses() : Collections.singletonList(info.getHost());
            for(InetAddress host:hosts) {
                if(!(host instanceof Inet4Address) || host.isAnyLocalAddress() || host.isMulticastAddress())continue;
                JSONObject ad=new JSONObject().put("family",family).put("authority",authority).put("world",world).put("schema",1).put("protocol",protocol).put("content",content);
                String answer=new JSONObject().put("address",host.getHostAddress()).put("port",info.getPort()).put("networkInterface",index).put("ad",ad).toString();
                if(!answers.contains(answer) && answers.size()<16)answers.add(answer);
            }
        } catch(Exception e) { error=-3; }
    }
    public synchronized int error() { return error; }
    public synchronized String take() { return answers.poll(); }
    public synchronized void close() {
        if(closed)return;closed=true;answers.clear();
        if(modern!=null && Build.VERSION.SDK_INT>=34)try { manager.unregisterServiceInfoCallback(modern); } catch(RuntimeException ignored) { }
        if(browsing && browse!=null)try { manager.stopServiceDiscovery(browse); } catch(RuntimeException ignored) { }
        modern=null;legacy=null;browse=null;browsing=false;
    }
}
