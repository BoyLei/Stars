//
//  GSPlatformModule.h
//  GSSDK
//
//  Created by yoka on 2024/4/29.
//

#import "GSBaseModule.h"

NS_ASSUME_NONNULL_BEGIN

@interface GSPlatformModule : GSBaseModule

// 登录
+ (void)login;

// 注销
+ (void)logout;

// 进入游戏
+ (void)enterGame:(NSDictionary *)gameData;

/**
 *  支付接口
 *  @param parameters  支付参数
 */
+ (void)pay:(NSDictionary *)parameters;

@end

NS_ASSUME_NONNULL_END
