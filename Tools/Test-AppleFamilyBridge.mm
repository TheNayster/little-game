#import <Foundation/Foundation.h>
#include <unistd.h>
#include <chrono>
#include <cstdio>
extern "C" {
void *LWFamilyBrowse(const char *);
int LWFamilyPump(void *);
char *LWFamilyTake(void *);
void LWFamilyStop(void *);
void LWFamilyFree(char *);
int LWPairingAdd(const char *);
}
// Links the exact iOS bridge on macOS. This checks native API behavior, not
// iOS privacy/Keychain entitlements or the Unity/DTLS data channel on a device.
int main(int argc,char **argv){@autoreleasepool {
    if(argc!=5)return 2;
    if(LWFamilyBrowse("bad")!=nullptr)return 3;
    // Reject malformed/server payloads before any Keychain API is reached.
    const char *bad[]={"null","{}","[]","{\"role\":\"server\"}","{\"role\":\"client\",\"privateKey\":42}","{\"role\":\"client\",\"members\":\"wrong\"}"};
    for(auto value:bad)if(LWPairingAdd(value)!=-50)return 4;
    double peak=0;int candidates=0;
    for(int pass=0;pass<3;pass++){
        void *browser=LWFamilyBrowse(argv[1]);if(!browser)return 5;
        bool found=false;auto start=std::chrono::steady_clock::now();
        while(std::chrono::duration<double>(std::chrono::steady_clock::now()-start).count()<15){
            auto before=std::chrono::steady_clock::now();int error=LWFamilyPump(browser);
            double took=std::chrono::duration<double,std::milli>(std::chrono::steady_clock::now()-before).count();peak=std::max(peak,took);
            if(error){LWFamilyStop(browser);fprintf(stderr,"DNS-SD error %d\n",error);return 6;}
            char *raw=LWFamilyTake(browser);
            if(raw){
                NSData *data=[NSData dataWithBytes:raw length:strlen(raw)];LWFamilyFree(raw);
                NSDictionary *answer=[NSJSONSerialization JSONObjectWithData:data options:0 error:nil];NSDictionary *ad=answer[@"ad"];
                found=[ad[@"authority"] isEqual:@(argv[1])] && [ad[@"family"] isEqual:@(argv[2])] && [ad[@"world"] isEqual:@(argv[3])] &&
                    [ad[@"schema"] intValue]==1 && [ad[@"protocol"] intValue]==3 && [ad[@"content"] intValue]==3 && [answer[@"port"] intValue]==atoi(argv[4]);
                if(found){candidates++;break;}
            }
            usleep(10000);
        }
        LWFamilyStop(browser);if(!found)return 7;
    }
    printf("{\"passed\":true,\"resolvedAndDisposed\":%d,\"malformedEnrollmentsRejectedWithoutKeychain\":6,\"peakPumpMs\":%.3f,\"scope\":\"macOS bridge to Windows advertisement; not iOS permission or Keychain qualification\"}\n",candidates,peak);
    return 0;
}}
