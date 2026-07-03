//
//  GSLogger.h
//  GSSDK
//
//  Created by YOKA on 2022/12/30.
//

#import <Foundation/Foundation.h>

#define GSLogLevel(level, fmt, ...) \
[GSLogger logLevel:level file:__FILE__ function:__PRETTY_FUNCTION__ line:__LINE__ format:(fmt), ##__VA_ARGS__]

#define GSLogDebug(fmt, ...) \
GSLogLevel(GSLogLevelDebug, (fmt), ##__VA_ARGS__)

#define GSLogInfo(fmt, ...) \
GSLogLevel(GSLogLevelInfo, (fmt), ##__VA_ARGS__)

#define GSLogWarn(fmt, ...) \
GSLogLevel(GSLogLevelWarn, (fmt), ##__VA_ARGS__)

#define GSLogError(fmt, ...) \
GSLogLevel(GSLogLevelError, (fmt), ##__VA_ARGS__)

NS_ASSUME_NONNULL_BEGIN

typedef NS_ENUM(NSUInteger, GSLogLevel) {
    GSLogLevelDebug = 0,
    GSLogLevelInfo,
    GSLogLevelWarn,
    GSLogLevelError
};

@interface GSLogger : NSObject

@property (nonatomic, assign, class) BOOL enableLog;

+ (void)logLevel:(GSLogLevel)level file:(const char *)file function:(const char *)function line:(NSUInteger)line format:(NSString *)format, ...;

@end

NS_ASSUME_NONNULL_END
