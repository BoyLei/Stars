//
//  GSDictionaryMerge.h
//  GSSDK
//
//  Created by YOKA on 2023/8/18.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GSDictionaryMerge : NSObject

+ (NSDictionary *)mergeDictionary:(NSDictionary *)target withSource:(NSDictionary *)source;

@end

NS_ASSUME_NONNULL_END
