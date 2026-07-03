//
//  UIDevice+GSAdd.h
//  GSCoreKit
//
//  Created by ritchie on 2024/6/4.
//

#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN

@interface UIDevice (GSAdd)
+ (BOOL)isSimulator;
+ (BOOL)isVPNConnected;
@end

NS_ASSUME_NONNULL_END
