//
//  NSDictionary+GSAdd.h
//  GSCoreKit
//
//  Created by ritchie on 2024/6/4.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface NSDictionary (GSBlock)
- (void)gs_each:(void (^)(id k, id v))block;
- (void)gs_eachKey:(void (^)(id k))block;
- (void)gs_eachValue:(void (^)(id v))block;
- (NSArray *)gs_map:(id (^)(id key, id value))block;
- (NSDictionary *)gs_pick:(NSArray *)keys;
- (NSDictionary *)gs_omit:(NSArray *)key;
@end


@interface NSDictionary (GSJSONString)
- (NSString *)gs_JSONString;
@end

NS_ASSUME_NONNULL_END
