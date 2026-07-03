//
//  GSPlatformDefines.h
//  GSSDK
//
//  Created by yoka on 2024/4/29.
//

#ifndef GSPlatformDefines_h
#define GSPlatformDefines_h

static NSString* const kServerId            = @"serverId";     // 游戏服务器id
static NSString* const kServerName          = @"serverName";   // 游戏服务器名
static NSString* const kRoleId              = @"roleId";       // 角色id
static NSString* const kRoleName            = @"roleName";     // 角色名
static NSString* const kRoleLevel           = @"roleLevel";    // 角色等级
static NSString* const kRoleCreateTime      = @"roleCreateTime";// 创建时间
static NSString* const kVipLevel            = @"vipLevel";     // vip等级
static NSString* const kRealServer            = @"realServer";     // 
static NSString* const kServerZone            = @"serverZone";  // 服务器地区
#pragma mark - 登录成功通知事件
static NSString *const kGSLoginSuccessNotification      = @"kGSLoginSuccessNotification"; // 登录成功发送广告通知

#pragma mark - EnterGame 成功通知
static NSString *const kGSSDKEnterGameNotification      = @"kGSSDKEnterGameNotification"; // 进入游戏调用通知

#pragma mark - LogoutGame 成功通知
static NSString *const kGSSDKLogoutGameNotification      = @"kGSSDKLogoutGameNotification"; // 退出游戏调用通知

#define GS_ERROR_UNKNOWN                -1          // 未知错误

#endif /* GSPlatformDefines_h */
