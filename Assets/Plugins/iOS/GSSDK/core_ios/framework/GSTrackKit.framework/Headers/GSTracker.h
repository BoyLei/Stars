//
//  GSAnalyticsKit.h
//  GSAnalytics
//
//  Created by YOKA on 2023/8/29.
//

#import <Foundation/Foundation.h>
#import "GSConstants.h"

@class GSUserInfo, GSTrackConfig;

NS_ASSUME_NONNULL_BEGIN

@interface GSTracker : NSObject

@property (nonatomic, copy, readonly, class) NSString *version;
@property (nonatomic, strong, readonly, class) GSTrackConfig *configOptions;
@property (nonatomic, strong, readonly, class) GSUserInfo *userInfo;

#pragma mark - init instance
- (instancetype)init NS_UNAVAILABLE;
/**
 此方法调用建议在应用启动时调用，即在 application:didFinishLaunchingWithOptions: 中调用
 @param configOptions 参数配置
 */
+ (void)startWithConfigOptions:(nonnull GSTrackConfig *)configOptions;

/// 设置用户信息
/// - Parameter userInfo: 标识事件用户
+ (void)setUserInfo:(nonnull GSUserInfo *)userInfo;

/// 清空追踪信息
+ (void)clearTrackerData;

#pragma mark - track event
+ (void)track:(GSTrackType)type;
+ (void)track:(GSTrackType)type name:(nullable NSString *)name;
+ (void)track:(GSTrackType)type name:(nullable NSString *)name parameters:(nullable NSDictionary<NSString *, id> *)parameters;

/**
 * @abstract
 * 强制试图把数据传到对应的服务器上
 * 主动调用 flush 接口，则不论限制条件是否满足，都尝试向服务器上传一次数据
 */
+ (void)flush;

/**
 * @abstract
 * 删除本地缓存的全部事件
 * 一旦调用该接口，将会删除本地缓存的全部事件，请慎用！
 */
+ (void)deleteAll;

@end

NS_ASSUME_NONNULL_END
