//
//  UIScrollView+GSKeyboardAvoidingAdditions.h
//  GSKeyboardAvoiding
//
//  Created by Michael Tyson on 30/09/2013.
//  Copyright 2015 A Tasty Pixel. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface UIScrollView (GSKeyboardAvoidingAdditions)
- (BOOL)GSKeyboardAvoiding_focusNextTextField;
- (void)GSKeyboardAvoiding_scrollToActiveTextField;

- (void)GSKeyboardAvoiding_keyboardWillShow:(NSNotification*)notification;
- (void)GSKeyboardAvoiding_keyboardWillHide:(NSNotification*)notification;
- (void)GSKeyboardAvoiding_updateContentInset;
- (void)GSKeyboardAvoiding_updateFromContentSizeChange;
- (void)GSKeyboardAvoiding_assignTextDelegateForViewsBeneathView:(UIView*)view;
- (UIView*)GSKeyboardAvoiding_findFirstResponderBeneathView:(UIView*)view;
-(CGSize)GSKeyboardAvoiding_calculatedContentSizeFromSubviewFrames;
@end
