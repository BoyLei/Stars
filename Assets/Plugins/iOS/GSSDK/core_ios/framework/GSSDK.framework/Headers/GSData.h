//
//  GSSDKData.h
//  GSSDK
//
//  Created by yoka on 2022/1/30.
//  Copyright © 2022 Boson. All rights reserved.
//

#import <Foundation/Foundation.h>

#define GSDataObj   [GSData sharedInstance]

@interface GSData : NSObject<NSCoding, NSCopying>

/// 账号类型（是否游客，GSSDK不准）
@property (nonatomic, copy) NSString *userType;//0：老用户，1：新用户
/// 账号类型（0：新、1：topData、2：anyData+anyExt、3：openData）
@property (nonatomic, copy) NSString *dataType;

/// 用户唯一ID
@property (nonatomic, copy) NSString *userID;
/// 用户的渠道ID
@property (nonatomic, copy) NSString *thirdUserId;
/// 登录成功返回的DATA数据（GS和TOP返回的数据格式会不同）
@property (nonatomic, copy) NSDictionary *loginData; // 二次验证登录返回的数据

// game传参
@property (nonatomic, copy) NSString *serverId;         // 游戏服务器ID
@property (nonatomic, copy) NSString *serverName;       // 游戏服务器名称
@property (nonatomic, copy) NSString *roleId;       // 游戏角色ID
@property (nonatomic, copy) NSString *roleName;     // 游戏角色名称
@property (nonatomic, copy) NSString *roleLevel;    // 游戏角色等级
@property (nonatomic, copy) NSString *roleCerateTime; // 游戏创建角时间

@property (nonatomic, copy) NSString *realserver; // 合服后玩家角色当前区服名称
@property (nonatomic, copy) NSString *serverZone; // 游戏服务器时区名称

/// 禁用 init 初始化
- (instancetype)init NS_UNAVAILABLE;
/// 禁用 new 初始化
+ (instancetype)new NS_UNAVAILABLE;

+ (instancetype)sharedInstance;

/// 清空账号数据
- (void)reset;

#pragma mark - 游戏数据更新

// 更新游戏信息
- (void)updateGameInfo:(NSDictionary *)gameData;
// 根据key值取参数
- (NSString *)getGameValue:(NSString *)keyName defaultValue:(NSString *)value;

@end


