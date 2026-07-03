//
//  GSAppConfig.h
//  GSSDK
//
//  Created by YOKA on 2023/4/17.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

#define AppConfigObj ((GSAppConfig*) [GSAppConfig sharedInstance])

static NSString *const kPlugins          = @"plugins";

@interface GSAppConfig : NSObject

@property (nonatomic, readonly, strong) NSDictionary *moduleMaps;   // 模块映射

@property (nonatomic, readonly, strong) NSDictionary *configData;

@property (nonatomic, readonly, strong) NSDictionary *loginData;
@property (nonatomic, readonly, strong) NSDictionary *paymentData;

///// 渠道/插件的参数
//@property (nonatomic, copy, readonly, class) NSDictionary *sdkPluginsParameter;

///appId
@property (nonatomic, copy, readonly, class) NSString *appId;
///commonAppId (通用新的appid)
@property (nonatomic, copy, readonly, class) NSString *commonAppId;
///appkey
@property (nonatomic, copy, readonly, class) NSString *appKey;
///渠道
@property (nonatomic, copy, readonly, class) NSString *channel;
///老游戏的渠道id
@property (nonatomic, copy, readonly, class) NSString *oldChannelId;
// 是否为国内
@property (nonatomic, readonly, assign, class) BOOL isInland;
//隐私组件
@property (nonatomic, copy, readonly, class) NSDictionary *privacyData;

+ (void)configInit;

+ (GSAppConfig *)sharedInstance;

/// 获取module_config 模块Dic内容 （仅限制模块单个SDK，不包含数据模块获取）
/// @param moduleKey 如：help，push，等
+ (NSDictionary *)getModuleConfigDicWithKey:(NSString *)moduleKey;
+(BOOL)readConfig;
@end

NS_ASSUME_NONNULL_END
