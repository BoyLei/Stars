//
//  GSAnalyticsModule.h
//  GSSDK
//
//  Created by yoka on 2024/5/7.
//

#import "GSBaseModule.h"

NS_ASSUME_NONNULL_BEGIN

typedef NS_ENUM (NSUInteger, GSAnalyticsCoreType) {
    GSAnalyticsCoreType_InitSuccess = 100,  // 初始化成功
    GSAnalyticsCoreType_Login = 102,        // 登录成功
    GSAnalyticsCoreType_EnterGame = 104,    // 进入游戏事件
    GSAnalyticsCoreType_RoleLevelUp = 105,  // 角色升级
    GSAnalyticsCoreType_Pay = 106           // 支付相关,支付点位需要传如stepID
};

typedef NS_ENUM (NSUInteger, GSAnalyticsCoreStepType) {
    GSAnalyticsCoreStepType_None = 0,                   // 无step状态
    GSAnalyticsCoreStepType_GetProductSuccess = 110,    // 查询商品成功
    GSAnalyticsCoreStepType_PayStart = 111,             // 发起支付
    GSAnalyticsCoreStepType_CreateOrder = 112,          // 下单
    GSAnalyticsCoreStepType_OrderSuccess = 113,         // 下单成功
    GSAnalyticsCoreStepType_PaySuccess = 200,           // 支付完成
};

@interface GSAnalyticsModule : GSBaseModule

+ (void)coreEvent:(GSAnalyticsCoreType)eventId stepId:(GSAnalyticsCoreStepType)stepId param:(NSDictionary *)param;

@end

NS_ASSUME_NONNULL_END
