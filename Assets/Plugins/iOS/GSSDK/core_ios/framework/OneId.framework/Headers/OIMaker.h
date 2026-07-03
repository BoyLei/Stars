//
//  OIMaker.h
//  OneId
//
//  Created by YOKA on 2024/4/12.
//

#import <Foundation/Foundation.h>
@class OIConfiguration;
@class OIResult;
NS_ASSUME_NONNULL_BEGIN

typedef void(^OIMakerResultCallback)(OIResult *result);

@interface OIMaker : NSObject
//+ (instancetype)sharedInstance;

+ (void)configure:(OIConfiguration *)configuration;


+ (void)oneId:(OIMakerResultCallback)callback;
+ (void)oneIdWithConfiguration:(OIConfiguration *)configuration
                      callback:(OIMakerResultCallback)complete;




+ (instancetype)alloc NS_UNAVAILABLE;
+ (instancetype)new NS_UNAVAILABLE;
- (instancetype)copy NS_UNAVAILABLE;
- (instancetype)mutableCopy NS_UNAVAILABLE;
@end

NS_ASSUME_NONNULL_END
