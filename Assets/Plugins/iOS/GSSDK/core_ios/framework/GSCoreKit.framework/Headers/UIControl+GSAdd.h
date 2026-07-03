//
//  UIControl+GSAdd.h
//  GSCoreKit
//
//  Created by ritchie on 2024/6/4.
//

#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN
typedef void (^UIControlGSActionBlock)(id weakSender);
@interface UIControl (GSActionBlocks)
- (void)gs_handleControlEvents:(UIControlEvents)controlEvents
                     withBlock:(UIControlGSActionBlock)actionBlock;
- (void)gs_removeActionBlocksForControlEvents:(UIControlEvents)controlEvents;
@end

NS_ASSUME_NONNULL_END
