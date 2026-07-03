//
//  TipsView.h
//
//  Created by zls on 2019/3/13.
//  Copyright © 2019年 zls. All rights reserved.
//

/**
 *      提示框
 *
 */
#import <UIKit/UIKit.h>

@interface GSTipsView : UIView

@property (nonatomic, strong) UILabel *tipsLab;
@property (nonatomic, strong) UIButton *trueBtn;

/** 1、带内容的提示框 */
- (id)initWithMsg:(NSString *)tipsStr;
/** 2、带有一个确定按钮的y提示框 */
- (id)initTipsViewAndButtonWithMsg:(NSString *)tipsStr;

- (id)initBlackBgWithMsg:(NSString *)tipsStr;

- (id)initWhiteBgWithMsg:(NSString *)tipsStr;

@end
