//
//  StringUtils.h
//
//  Created by zls on 19/5/30.
//  Copyright © 2019年 zls. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@interface GSStringUtils : NSObject

+ (BOOL)isBlankString:(NSString *)string;
/**
 *  计算文本的宽高
 *
 *  @param str     需要计算的文本
 *  @param font    文本显示的字体
 *  @param maxSize 文本显示的范围
 *
 *  @return 文本占用的真实宽高
 */
+ (CGSize)sizeWithString:(NSString *)str font:(UIFont *)font maxSize:(CGSize)maxSize;

+ (NSString *)TimeLongToString:(NSString *)str;
/**
 *  判断注册用户名、密码
 *  6-15位数字或英文字符
 */
+ (BOOL)judgePassWordLegal:(NSString *)pass;

+ (BOOL)checkSmsCode:(NSString *)smsCode;

+(BOOL)checkEmailNum:(NSString *)emailNum;

+ (BOOL)checkMobileNum:(NSString *)mobileNum;
/** 设置PX与iOS尺寸高度的转换 */
+ (CGFloat)heightWithPX:(int)px;

/** 设置PX与iOS尺寸宽度的转换 */
+ (CGFloat)weightWithPX:(int)px;

/** 字体大小与UI给的px对应关系 */
+ (CGFloat)sizeWithPX:(int)px;

/** 设置PX与iOSphone5尺寸高度的转换 */
+ (CGFloat)heightWithIPhone5PX:(int)px;

/** 设置PX与iOSphone5尺寸宽度的转换 */
+ (CGFloat)weightWithIPhone5PX:(int)px;

/** 字体大小与UI给的px对应关系 */
+ (CGFloat)sizeWithIPhone5PX:(int)px;

/**
 *  不可定义透明度。默认1，即不透明
 *
 *  @param stringToConvert 十六进制颜色值
 *
 *  @return RGB颜色
 */
+ (UIColor *)getColorWithHexString:(NSString *)stringToConvert;

/** 判断当前设备是不是iPhone X系列 */
+ (BOOL)isIPhponeX;

/** 判断当前设备是不是iPAD */
+ (BOOL)isIPad;

+ (NSString *)getPreferredLanguage;

/** 获取当前手机设置的语言 */
+ (NSString *)getPreferredLanguageAndIsHttpSortParmas:(BOOL)sortParmas;

//地区码
+ (NSString *)getCurrentArea;

/** 获取错误码要对应的设置的语言 */

+ (NSString *)getHttpErrorLanguage;

//web网页需要的语言码
+ (NSString *)getWebLanguage;

//web网页需要的地区码
+ (NSString *)getWebAreaCode;

/** 字典转json字符串方法 */
+ (NSString *)convertToJsonData:(NSDictionary *)dict;

+ (NSString *)loggerForJsonData:(NSDictionary *)dict;

/** JSON字符串转化为字典 */
+ (NSDictionary *)dictionaryWithJsonString:(NSString *)jsonString;


+ (void)setFacilityAccount:(NSString *)account;
+ (void)setFacilityToken:(NSString *)token;
+ (void)setFacilityChannel:(NSString *)channel;


+ (CGFloat)contentWidthWithText:(NSString*)text height:(float)height fontSizes:(UIFont *)font;

+ (CGFloat)contentCellHeightWithText:(NSString*)text width:(float)width font:(UIFont *)font;

+ (NSString *)deleteStringFormString:(NSString *)fromString andDeleteString:(NSString *)deleteString;

/// 手机号或者邮箱号弄成****隐私掉
/// @param string 传的字符串
+ (NSString*)stringChangeStarString:(NSString *)string andIsEmail:(BOOL)isEmail;


+(CABasicAnimation *)opacityAfter:(NSNumber *)after before:(NSNumber *)before durTimes:(float)time Rep:(float)repertTimes;

+(CABasicAnimation *)scale:(NSNumber *)Multiple orgin:(NSNumber *)orginMultiple durTimes:(float)time Rep:(float)repertTimes;

@end
