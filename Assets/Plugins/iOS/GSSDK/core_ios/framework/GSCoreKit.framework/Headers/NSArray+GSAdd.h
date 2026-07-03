//
//  NSArray+GSAdd.h
//  GSCoreKit
//
//  Created by ritchie on 2024/5/30.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface NSArray (GSBlock)
- (void)gs_each:(void (^)(id object))block;
- (void)gs_eachWithIndex:(void (^)(id object, NSUInteger index))block;
- (NSArray *)gs_map:(id (^)(id object))block;
- (NSArray *)gs_filter:(BOOL (^)(id object))block;
- (NSArray *)gs_reject:(BOOL (^)(id object))block;
- (id)gs_detect:(BOOL (^)(id object))block;
- (id)gs_reduce:(id (^)(id accumulator, id object))block;
- (id)gs_reduce:(id)initial withBlock:(id (^)(id accumulator, id object))block;
@end

@interface NSArray (GSAddSafe)
- (id)gs_safeObjectAtIndex:(NSUInteger)index;
@end

NS_ASSUME_NONNULL_END
