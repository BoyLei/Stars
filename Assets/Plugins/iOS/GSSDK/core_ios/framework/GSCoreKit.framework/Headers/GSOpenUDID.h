//
//  GSOpenUDID.h
//  GSCoreKit
//
//  Created by yoka on 2024/5/16.
//

#import <Foundation/Foundation.h>
#define GSOpenUDIDErrorNone          0
#define GSOpenUDIDErrorOptedOut      1
#define GSOpenUDIDErrorCompromised   2
@interface GSOpenUDID : NSObject{
}
+ (NSString*) getDeviceUDID;
+ (NSString*) valueWithError:(NSError**)error;
+ (void) setOptOut:(BOOL)optOutValue;
@end
