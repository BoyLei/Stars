//
//  GSCallTrack.h
//  GSSDK
//
//  Created by yoka on 2024/5/7.
//

#import <Foundation/Foundation.h>
#import "GSAnalyticsModule.h"

NS_ASSUME_NONNULL_BEGIN

@interface GSCallTrack : NSObject

/// GS行为日志 track 埋点
/// @param  type 日志类型
/// @param event 事件id
/// @param details details description
void GSTrackCall(NSInteger type, NSString *event, NSDictionary *details);

/// 给魔方打核心点位
void GSAnalyticsCall(GSAnalyticsCoreType event, GSAnalyticsCoreStepType stepId, NSDictionary *details);


@end

NS_ASSUME_NONNULL_END
