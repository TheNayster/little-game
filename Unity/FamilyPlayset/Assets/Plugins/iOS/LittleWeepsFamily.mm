#import <Foundation/Foundation.h>
#import <Security/Security.h>
#include <dns_sd.h>
#include <arpa/inet.h>
#include <poll.h>
#include <chrono>
#include <vector>
#include <memory>
#include <string>
#include <cstring>

// Small polling bridge: native DNS-SD owns discovery, Unity owns the frame loop.
// All calls/callbacks are serialized on that loop, with no reverse managed/AOT
// callbacks and no native worker thread that can outlive the app's foreground.
namespace {
struct Browser;
struct Operation {
    Browser *owner;
    DNSServiceRef ref=nullptr;
    uint32_t nic=0;
    uint16_t port=0;
    double deadline;
    bool done=false;
    std::string key;
    std::string advertisement;
    explicit Operation(Browser *b):owner(b),deadline(1e30){}
    ~Operation(){if(ref)DNSServiceRefDeallocate(ref);}
};
static double clockSeconds(){return std::chrono::duration<double>(std::chrono::steady_clock::now().time_since_epoch()).count();}
static std::string jsonString(NSDictionary *value){NSData *data=[NSJSONSerialization dataWithJSONObject:value options:0 error:nil];return data?std::string((const char *)data.bytes,data.length):std::string();}
static NSString *text(const void *bytes,size_t count){return [[NSString alloc] initWithBytes:bytes length:count encoding:NSUTF8StringEncoding];}
static NSString *txtValue(const void *txt,uint16_t length,const char *key){uint8_t n=0;const void *value=TXTRecordGetValuePtr(length,txt,key,&n);return value?text(value,n):nil;}
struct Browser {
    std::string authority;
    std::vector<std::unique_ptr<Operation>> operations;
    std::vector<std::string> answers;
    int error=0;
    explicit Browser(const char *id):authority(id){}
    void clean(){for(auto i=operations.begin();i!=operations.end();)if((*i)->done || clockSeconds()>(*i)->deadline)i=operations.erase(i);else ++i;}
};
static void addressReply(DNSServiceRef,uint32_t flags,uint32_t nic,DNSServiceErrorType error,const char *,const sockaddr *address,uint32_t ttl,void *context){
    auto op=(Operation *)context;auto b=op->owner;
    if(error){b->error=error;op->done=true;return;}
    if(!(flags&kDNSServiceFlagsAdd) || !ttl || !address || address->sa_family!=AF_INET || b->answers.size()>=16)return;
    char host[INET_ADDRSTRLEN]={};auto ip=(const sockaddr_in *)address;
    if(!inet_ntop(AF_INET,&ip->sin_addr,host,sizeof(host)))return;
    // Endpoint metadata remains untrusted; C# matches enrollment and DTLS
    // validates the server. No credentials are ever present in TXT records.
    NSDictionary *ad=[NSJSONSerialization JSONObjectWithData:[NSData dataWithBytes:op->advertisement.data() length:op->advertisement.size()] options:0 error:nil];
    b->answers.push_back(jsonString(@{@"address":@(host),@"port":@(op->port),@"networkInterface":@(nic),@"ad":ad}));
}
static void resolveReply(DNSServiceRef,uint32_t,uint32_t nic,DNSServiceErrorType error,const char *,const char *host,uint16_t port,uint16_t length,const unsigned char *txt,void *context){
    auto source=(Operation *)context;auto b=source->owner;source->done=true;
    if(error){b->error=error;return;}
    if(!host || length>1024 || ntohs(port)<1024 || b->operations.size()>=16)return;
    NSString *family=txtValue(txt,length,"family"),*authority=txtValue(txt,length,"authority"),*world=txtValue(txt,length,"world");
    NSString *version=txtValue(txt,length,"v"),*protocol=txtValue(txt,length,"protocol"),*content=txtValue(txt,length,"content");
    if(!family || !authority || !world || !version || !protocol || !content)return;
    if(family.length!=32 || authority.length!=32 || world.length!=32 || ![authority isEqualToString:@(b->authority.c_str())])return;
    auto op=std::make_unique<Operation>(b);op->nic=nic;op->port=ntohs(port);op->deadline=clockSeconds()+8;op->key=source->key;
    op->advertisement=jsonString(@{@"family":family,@"authority":authority,@"world":world,@"schema":@([version intValue]),@"protocol":@([protocol intValue]),@"content":@([content intValue])});
    // IPv4 first, matching the qualified Windows path. IPv6/scoped-address
    // routing is a separate recorded acceptance item, not silently assumed.
    error=DNSServiceGetAddrInfo(&op->ref,0,nic,kDNSServiceProtocol_IPv4,host,addressReply,op.get());
    if(error)b->error=error;else b->operations.push_back(std::move(op));
}
static void browseReply(DNSServiceRef,uint32_t flags,uint32_t nic,DNSServiceErrorType error,const char *name,const char *type,const char *domain,void *context){
    auto b=((Operation *)context)->owner;
    if(error){b->error=error;return;}
    if(!name || std::string(name)!="LW-"+b->authority || !type || strcmp(type,"_lw-playset._udp.") || !domain || strcmp(domain,"local."))return;
    auto key=std::to_string(nic)+":"+name;
    if(!(flags&kDNSServiceFlagsAdd)){for(auto &op:b->operations)if(op->key==key)op->done=true;b->answers.clear();return;}
    if(b->operations.size()>=16)return;
    for(auto &op:b->operations)if(op->key==key && !op->done)return;
    auto op=std::make_unique<Operation>(b);op->key=key;op->deadline=clockSeconds()+8;
    error=DNSServiceResolve(&op->ref,0,nic,name,type,domain,resolveReply,op.get());
    if(error)b->error=error;else b->operations.push_back(std::move(op));
}
static NSDictionary *keyQuery(){return @{(__bridge id)kSecClass:(__bridge id)kSecClassGenericPassword,(__bridge id)kSecAttrService:@"com.littleweeps.familyplayset.enrollment.v1",(__bridge id)kSecAttrAccount:@"current-player",(__bridge id)kSecAttrSynchronizable:@NO};}
}

extern "C" {
void *LWFamilyBrowse(const char *authority){
    if(!authority || strlen(authority)!=32)return nullptr;
    auto b=std::make_unique<Browser>(authority);auto op=std::make_unique<Operation>(b.get());
    b->error=DNSServiceBrowse(&op->ref,0,0,"_lw-playset._udp","local.",browseReply,op.get());
    if(!b->error)b->operations.push_back(std::move(op));
    return b.release();
}
int LWFamilyPump(void *handle){
    auto b=(Browser *)handle;if(!b)return -1;
    std::vector<Operation *> snapshot;for(auto &op:b->operations)snapshot.push_back(op.get());
    for(auto op:snapshot){
        if(op->done)continue;
        pollfd descriptor{DNSServiceRefSockFD(op->ref),POLLIN,0};int result=poll(&descriptor,1,0);
        if(result<0 || descriptor.revents&(POLLERR|POLLHUP|POLLNVAL)){b->error=-1;op->done=true;continue;}
        if(result>0 && descriptor.revents&POLLIN){int error=DNSServiceProcessResult(op->ref);if(error){b->error=error;op->done=true;}}
    }
    b->clean();return b->error;
}
char *LWFamilyTake(void *handle){auto b=(Browser *)handle;if(!b || b->answers.empty())return nullptr;char *answer=strdup(b->answers.front().c_str());b->answers.erase(b->answers.begin());return answer;}
void LWFamilyStop(void *handle){delete (Browser *)handle;}
void LWFamilyFree(char *text){if(text){memset(text,0,strlen(text));free(text);}}
char *LWPairingRead(int *status){
    NSMutableDictionary *query=[keyQuery() mutableCopy];query[(__bridge id)kSecReturnData]=@YES;query[(__bridge id)kSecMatchLimit]=(__bridge id)kSecMatchLimitOne;
    CFTypeRef result=nullptr;OSStatus code=SecItemCopyMatching((__bridge CFDictionaryRef)query,&result);if(status)*status=(int)code;
    if(code!=errSecSuccess)return nullptr;
    NSData *value=CFBridgingRelease(result);
    if(value.length<1 || value.length>32768){if(status)*status=errSecDecode;return nullptr;}
    NSString *json=text(value.bytes,value.length);if(!json){if(status)*status=errSecDecode;return nullptr;}return strdup(json.UTF8String);
}
int LWPairingAdd(const char *json){
    if(!json || strlen(json)>32768)return errSecParam;
    NSData *data=[NSData dataWithBytes:json length:strlen(json)];
    id object=[NSJSONSerialization JSONObjectWithData:data options:0 error:nil];
    if(![object isKindOfClass:[NSDictionary class]])return errSecParam;
    NSDictionary *record=object;
    if(![record[@"role"] isEqual:@"client"])return errSecParam;
    id secret=record[@"privateKey"],members=record[@"members"];
    if(secret && secret!=[NSNull null] && (![secret isKindOfClass:[NSString class]] || [secret length]>0))return errSecParam;
    if(members && members!=[NSNull null] && (![members isKindOfClass:[NSArray class]] || [members count]>0))return errSecParam;
    NSMutableDictionary *query=[keyQuery() mutableCopy];query[(__bridge id)kSecValueData]=data;
    query[(__bridge id)kSecAttrAccessible]=(__bridge id)kSecAttrAccessibleWhenUnlockedThisDeviceOnly;
    OSStatus code=SecItemAdd((__bridge CFDictionaryRef)query,nullptr);
    if(code==errSecDuplicateItem){
        // Retrying the same USB provision is harmless. A different enrollment
        // cannot overwrite an existing child's identity without a future,
        // explicit parent-controlled replacement flow.
        int status=0;char *prior=LWPairingRead(&status);bool same=prior && !strcmp(prior,json);LWFamilyFree(prior);return same?0:(int)errSecDuplicateItem;
    }
    return (int)code;
}
void LWExcludeEnrollmentBackup(const char *path){if(path){NSURL *url=[NSURL fileURLWithPath:@(path)];[url setResourceValue:@YES forKey:NSURLIsExcludedFromBackupKey error:nil];}}
}
