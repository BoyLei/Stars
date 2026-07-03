//
//  GSSDKUtility.h
//  GSMobileSDK
//
//  Created by yoka on 2021/12/21.
//  Copyright © 2021 Boson. All rights reserved.
//

#import <Foundation/Foundation.h>

extern NSString * const kGSEventDateFormatter;

@interface GSSDKUtility : NSObject

/**
 md5加密

 @param originalStr 待加密的字符串
 @return 加密后的字符串
 */
+ (NSString *)md5String:(NSString *)originalStr;


/**
 3DES加密

 @param originalStr 待加密的字符串
 @param key 加密key
 @param iv 可选初始化向量(如果不存在写nil, 不可@"")
 @return 加密后的字符串
 */
+ (NSString *)do3DESEncrypt:(NSString *)originalStr key:(NSString *)key ivInitVector:(NSString *)iv;

/**
 判断是否包含特殊字符

 @param string 待检测的字符串
 @return YES:是    NO:不是
 */
+ (BOOL)isContainSpecialCharacter:(NSString *)string;

/**
 验证对象是否为空(nil class)

 @param object 待验证对象
 @return YES:是    NO:不是
 */
+ (BOOL)isNullClassObject:(NSObject *)object;

/**
 验证string是否为空

 @param string 待验证string
 @return YES:是    NO:不是
 */
+ (BOOL)isBlankString:(NSString *)string;


/**
 验证数组是否为空

 @param array 待验证的数组
 @return YES:是    NO:不是
 */
+ (BOOL)isEmptyArray:(NSArray *)array;


/**
 验证字典是否为空

 @param dictionary 待验证的字典
 @return YES:是    NO:不是
 */
+ (BOOL)isEmptyDictionary:(NSDictionary *)dictionary;


/**
 验证data是否为空

 @param data 待验证的data
 @return YES:是    NO:不是
 */
+ (BOOL)isNullData:(NSData *)data;

+(NSData *)returnDataWithDictionary:(NSDictionary*)dict;

+ (NSDictionary*)returnDictionaryWithDataPath:(NSData*)data;


/**
*  @abstract
*  获取 NSDateFormatter 单例对象
*
*  @param string 日期格式
*
*  @return 返回 NSDateFormatter 单例对象
*/
+ (NSDateFormatter *)dateFormatterFromString:(NSString *)string;

@end

