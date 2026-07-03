//
//  GSAppProxy.h
//  GSAppProxyDemo
//
//  Created by 帅乐 on 2022/5/19.
//

#import <Foundation/Foundation.h>
#import "GSAppDelegate.h"

NS_ASSUME_NONNULL_BEGIN

@interface GSAppProxy : NSObject

@property (nonatomic, copy, readonly, class) NSSet<id<GSAppDelegate>> *delegates;

+ (instancetype)sharedProxy;
- (instancetype)init NS_UNAVAILABLE;

- (void)registDelegate:(id<GSAppDelegate>)delegate;
- (void)cancelDelegate:(id<GSAppDelegate>)delegate;

@end

NS_ASSUME_NONNULL_END
