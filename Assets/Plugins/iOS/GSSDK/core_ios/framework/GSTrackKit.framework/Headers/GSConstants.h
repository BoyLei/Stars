//
//  GSConstants.h
//  GSAnalytics
//
//  Created by YOKA on 2023/8/29.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

typedef NS_ENUM(NSInteger, GSTrackType) {
    GSTrackTypeActionBase = 1001, //行为日志基础类
    GSTrackTypeActionAccount = 1002, //行为日志账号类
    GSTrackTypeAccountPay = 1003, //行为日志支付类
    GSTrackTypeTicket = 1004, //票据日志类
    GSTrackType199 = 1005, //199日志类
    GSTrackTypeCrash = 1006, //崩溃报告
    GSTrackTypeProtocol = 1007, //关键协议
    GSTrackTypeRuntime = 1008, //运行时日志
    GSTrackTypeWarning = 1009, //警告类
    GSTrackTypeError = 1010, //错误类
};

@interface GSConstants : NSObject

@end

NS_ASSUME_NONNULL_END
