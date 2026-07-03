//
//  GSSDKCallback.h
//  GSSDK
//
//  Created by yoka on 2024/4/29.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

static NSString *const GSSDKInitSuccessNotification = @"GSSDKInitSuccessNotification";

@interface GSSDKCallback : NSObject

+ (GSSDKCallback *) sharedInstance;

// 初始化完成回调
- (void)GSInitFinish:(int)ret withParam:(nullable NSString *)param;

// 登录二次校验
- (void)GSLoginPlatform:(int)ret withParam:(nullable NSString *)param;

// 支付结果回调
- (void)GSPayResult:(int)ret withParam:(nullable NSString *)param;

// 查询结果回调
- (void)GSQueryPayResult:(int)ret withParam:(nullable NSString *)param;

// 注销回调
- (void)GSLogout:(int)ret withMsg:(NSString *)msg;

// 通用callback接口
- (void)GSCallBack:(NSString *)funcName code:(int)code msg:(NSString *)msg data:(NSDictionary *)data;

@end

NS_ASSUME_NONNULL_END
