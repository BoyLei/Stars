//
//  ServerMangager.h
//  GS
//
//  Created by sen on 2021/12/14.
//

#import <Foundation/Foundation.h>

@class GSPayParams;
typedef void (^GSGetPluginsActiveCallback)(int ret, NSString *response);
typedef void (^GSGetOrderCallback)(int ret, NSString *response,NSDictionary *orderDic);
typedef void (^GSGetUserInfoCallback)(int ret, NSString *response);

@interface GSServerManager : NSObject
+ (void)sendPluginsActivate:(NSString*)pluginTypeId pluginsActiveCallback:(GSGetPluginsActiveCallback)handler;

+ (void)getOrderId:(GSPayParams *)params payOrderCallback:(GSGetOrderCallback)handler;

/// 二次验证接口
+ (void)getUserInfo:(NSString *)token
   userInfoCallback:(GSGetUserInfoCallback)handler;
+ (void)getUserInfoWithUserId:(NSString *)userid
                        token:(NSString *)token
                    otherData:(NSString *)userdata
             userInfoCallback:(GSGetUserInfoCallback)handler;

@end


