//
//  GSConfigOptions.h
//  GSAnalytics
//
//  Created by YOKA on 2023/8/29.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

/// track配置项
@interface GSTrackConfig : NSObject

@property (nonatomic, copy) NSString *app_id;
@property (nonatomic, copy) NSString *channel_id;

@property (nonatomic, copy) NSString *domain; //上报域名地址，默认http://10.235.99.80:12346
@property (nonatomic, assign) BOOL isRealtime; //是否实时上报, 默认NO
@property (nonatomic, assign) NSTimeInterval periodicTimerMinute; //定时上报时间单位分, 默认5分钟
@property (nonatomic, assign) BOOL isInland;
@property (nonatomic, copy) NSString *oneIdRequestUrl;

@property (nonatomic, assign) NSInteger flushItem; //上报条数, 默认10条
@property (nonatomic, assign) BOOL disableLog; //关闭 log 日志, 默认NO
@property (nonatomic, assign) BOOL disableSDK; //禁用 SDK。设置后，SDK 将不采集事件，不发送网络请求，默认为NO

@end

NS_ASSUME_NONNULL_END
