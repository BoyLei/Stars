//
//  UIViewController+GSAdd.h
//  GSCoreKit
//
//  Created by ritchie on 2024/6/4.
//

#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN

@interface UIViewController (GSAdd)

+ (nullable UIViewController *)gs_topMostController;
+ (nullable UIViewController *)gs_currentViewController;
//- (nullable UIWindow *)gs_keyWindow;
@end

NS_ASSUME_NONNULL_END
