#import "GSMBProgressHUD.h"

@interface GSMBProgressHUD (utils)

+ (GSMBProgressHUD *) showProgressHUD:(UIView *)view;
+ (void) hideProgressHUD:(UIView *)view;
+ (void) showToast:(UIView *)view;

@end
