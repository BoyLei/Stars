//
//  GSUserInfo.h
//  GSAnalytics
//
//  Created by YOKA on 2023/8/29.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GSUserInfo : NSObject

@property (nonatomic, copy) NSString *role_id;
@property (nonatomic, copy) NSString *server_zone;//区服的区域
@property (nonatomic, copy) NSString *server;//区服的ID
@property (nonatomic, copy) NSString *login_from;//启动方式、来源
@property (nonatomic, copy) NSString *user_id;//用户ID
@property (nonatomic, copy) NSString *user_lev;//角色等级
@property (nonatomic, copy) NSString *yoka_id;//认证系统游卡ID/第三方ID
@property (nonatomic, copy) NSString *user_type;//账号类型注册方式等(微信注册、手机注册、游客登录等)
@property (nonatomic, copy) NSString *realserver;//和服后玩家角色当前区服

@end

NS_ASSUME_NONNULL_END
