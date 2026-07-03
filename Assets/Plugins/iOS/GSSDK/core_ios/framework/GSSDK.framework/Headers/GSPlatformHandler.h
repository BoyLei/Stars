//
//  GSPlatformHandler.h
//  GSSDK
//
//  Created by yoka on 2024/4/29.
//

#import <Foundation/Foundation.h>
#import "GSSDKCallback.h"
#import "GSConstant.h"
#import "GSData.h"
#import "GSPlatformDefines.h"
#import <GSCoreKit/GSCoreKit.h>
#import "GSPayParams.h"
#import "GSServerManager.h"

#define GSPlatformObj       [GSPlatformHandler sharedInstance]

NS_ASSUME_NONNULL_BEGIN

@interface GSPlatformHandler : NSObject

@property (nonatomic, strong) GSSDKCallback *gsSdkCallback;

+ (GSPlatformHandler *)sharedInstance;

// init platform
- (void)initPlatformDic:(NSDictionary *)configData;

// Login
- (void)login:(nullable NSDictionary *)dic;

// LogOut
- (void)logout;

// EnterGame
- (void)enterGame:(GSData *)gameData;

// 创建角色
- (void)createRole:(GSData *)gameData;

// 角色升级
- (void)roleLevelUp:(GSData *)gameData;

// 通用语言设置
- (void)setLang:(NSDictionary *)dic;

// pay
- (void)purchaseBridge:(NSDictionary *)parameters;

// 获取订单号
- (void)getOrder:(GSPayParams *)param block:(GSGetOrderCallback)callback;

// 登录成功后二次校验
- (void)loginPlatformWithUserId:(NSString *)userId token:(NSString *)token param:(NSDictionary *)param;

@end

NS_ASSUME_NONNULL_END
