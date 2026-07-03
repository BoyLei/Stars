//
//  GlobalData.h

//
//  Created by zls on 2019/6/15.
//  Copyright © 2019年 zls. All rights reserved.
//

#ifndef GSGlobalData_h
#define GSGlobalData_h

#import "GSStringUtils.h"

//是否竖屏
#define GS_isPortrait ([[UIApplication sharedApplication] statusBarOrientation] == UIInterfaceOrientationPortrait || [[UIApplication sharedApplication] statusBarOrientation] == UIInterfaceOrientationPortraitUpsideDown)
//是否横屏
#define GS_isLandscape !GS_isPortrait
//竖屏幕宽高
#define GS_DEF_ScreenWidth          [UIScreen mainScreen].bounds.size.width
#define GS_DEF_ScreenHeight         [UIScreen mainScreen].bounds.size.height

#define GS_DEF_SCREEN_MAX_LENGTH               (MAX(GS_DEF_ScreenWidth, GS_DEF_ScreenHeight))
#define GS_DEF_SCREEN_MIN_LENGTH               (MIN(GS_DEF_ScreenWidth, GS_DEF_ScreenHeight))
#define GS_DEF_SCREEN_RATE                 ((GS_DEF_ScreenHeight>GS_DEF_ScreenWidth)?GS_DEF_ScreenWidth:GS_DEF_ScreenHeight)/375//以iphone6为标准尺寸
#define IS_GSSDKIPHONE_X      (((int)((GS_DEF_SCREEN_MAX_LENGTH/GS_DEF_SCREEN_MIN_LENGTH)*100) == 216)?YES:NO)
#define GSSDKStatusBarAndNavigationBarHeight (IS_GSSDKIPHONE_X ? 88.f : 64.f)
#define GSSDKStatusBarHeight    (IS_GSSDKIPHONE_X ? 44 : 20)
// px与iOS端的换算比例
#define kHPX(px)    [GSStringUtils heightWithPX:px]
#define kWPX(px)    [GSStringUtils weightWithPX:px]

// px与iOS phone5端的换算比例
#define kHNPX(px)    [GSStringUtils heightWithIPhone5PX:px]
#define kWNPX(px)    [GSStringUtils weightWithIPhone5PX:px]
#define iPhoneXSolveQuestion ([UIScreen instancesRespondToSelector:@selector(currentMode)] ? CGSizeEqualToSize(CGSizeMake(1125, 2436), [[UIScreen mainScreen] currentMode].size) : NO)

// 主要色调
#define kHexColor(str)   [GSStringUtils getColorWithHexString:str]
#define kNavColor        [UIColor colorWithRed:25 / 255.0 green:177 / 255.0 blue:234 / 255.0 alpha:1.00]   // 导航栏颜色
#define kBackColor       [UIColor colorWithRed:246 / 255.0 green:246 / 255.0 blue:246 / 255.0 alpha:1.00]  // 页面背景色
#define kRGBColor(r,g,b) [UIColor colorWithRed:r / 255.0 green:g / 255.0 blue:b / 255.0 alpha:1.00]
#define kNormalColor     kRGBColor(153, 153, 153)

// 字体
#define kSizePX(px)       [GSStringUtils sizeWithPX:px]
#define kSizeNPX(px)       [GSStringUtils sizeWithIPhone5PX:px]

#define kName_Medium      @"PingFang-SC-Medium"
#define kName_Regular     @"PingFang-SC-Regular"
#define kName_Bold        @"PingFangSC-Semibold"
#define kName_YaHei       @"MicrosoftYaHei"
#define kName_Thin      @"PingFangSC-Light"
#define kName_AvenirNext      @"AvenirNext-Heavy"

#define kFont(name,size)  [UIFont fontWithName:name size:size]
#define PlaceFontSize       [UIFont systemFontOfSize:13*GS_DEF_ScreenWidth/375]

//适配iPhoneX
#define kDevice_Is_iPhoneX  [GSStringUtils isIPhponeX]
//适配iPAD
#define kDevice_Is_iPad  [GSStringUtils isIPad]
#define kStatusBarHeight    kDevice_Is_iPhoneX ? 44 : 20
#define kStatusUIHeight     kDevice_Is_iPhoneX ? 44 : 30

#define checkNull(__X__)    (__X__) == [NSNull null] || (__X__) == nil ? @"" : [NSString stringWithFormat:@"%@", (__X__)]

// 定义一个弱引用
#define WeakSelf(weakSelf)          __weak __typeof(&*self)weakSelf = self
#define StrongSelf(strongSelf) __strong __typeof(weakSelf) strongSelf = weakSelf

/** 获取当前手机设置的语言 */
#define kLanguages                  [GSStringUtils getPreferredLanguage]

#endif /* GlobalData_h */
