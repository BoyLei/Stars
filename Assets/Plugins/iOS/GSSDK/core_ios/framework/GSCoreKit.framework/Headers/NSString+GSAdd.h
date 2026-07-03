//
//  NSString+GSAdd.h
//  GSCoreKit
//
//  Created by ritchie on 2024/6/3.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface NSString (GSBase64)
+ (NSString *)gs_stringWithBase64EncodedString:(NSString *)string;
- (NSString *)gs_base64EncodedStringWithWrapWidth:(NSUInteger)wrapWidth;
- (NSString *)gs_base64EncodedString;
- (NSString *)gs_base64DecodedString;
- (NSData *)gs_base64DecodedData;
@end

@interface NSString (GSHASH)

- (NSString *)gs_md5String;

@end

@interface NSString (GSDictionaryValue)
/**
 *  @brief  JSON字符串转成NSDictionary
 *  @return NSDictionary
 */
- (NSDictionary *)gs_dictionaryValue;
@end



@interface NSString (GSURLEncode)
/**
 *  @brief  urlEncode
 *
 *  @return urlEncode 后的字符串
 */
- (NSString *)gs_urlEncode;
/**
 *  @brief  urlEncode
 *
 *  @param encoding encoding模式
 *
 *  @return urlEncode 后的字符串
 */
- (NSString *)gs_urlEncodeUsingEncoding:(NSStringEncoding)encoding;
/**
 *  @brief  urlDecode
 *
 *  @return urlDecode 后的字符串
 */
- (NSString *)gs_urlDecode;
/**
 *  @brief  urlDecode
 *
 *  @param encoding encoding模式
 *
 *  @return urlDecode 后的字符串
 */
- (NSString *)gs_urlDecodeUsingEncoding:(NSStringEncoding)encoding;

/**
 *  @brief  url query转成NSDictionary
 *
 *  @return NSDictionary
 */
- (NSDictionary *)gs_dictionaryFromURLParameters;
@end




@interface NSString (GSUUID)
/**
 *  @brief  获取随机 UUID 例如 E621E1F8-C36C-495A-93FC-0C247A3E6E5F
 *
 *  @return 随机 UUID
 */
+ (NSString *)gs_UUID;
/**
 *
 *  @brief  毫秒时间戳 例如 1443066826371
 *
 *  @return 毫秒时间戳
 */
+ (NSString *)gs_UUIDTimestamp;

@end
NS_ASSUME_NONNULL_END
