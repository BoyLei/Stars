//
//  GSCCID.h
//  GSCoreKit
//
//  Created by ritchie on 2024/5/30.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GSCCID : NSObject

+ (void)setAppId:(NSString *)appId;

+ (NSString *)CCIDv0;
+ (NSString *)CCIDv1;
+ (NSString *)CCIDv0Version;
+ (NSString *)CCIDv1Version;

+ (NSArray<NSDictionary *> *)CCIDs;
@end

NS_ASSUME_NONNULL_END
