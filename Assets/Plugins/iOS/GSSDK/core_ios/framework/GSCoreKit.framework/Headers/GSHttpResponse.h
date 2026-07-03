//
//  GSHttpResponse.h
//  TestDemo
//
//  Created by YOKA on 2022/8/23.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GSHttpResponse : NSObject

/**
 http响应状态码
 */
@property (nonatomic, assign) NSInteger statusCode;

/**
 响应体
 */
@property (nonatomic, copy) NSData *bodyData;

/**
 响应体转为 NSString
 */
@property (nonatomic, copy) NSString *bodyString;

/**
 响应体转为 JSON Object
 */
@property (nonatomic, strong) id bodyObject;

/**
 是否有效(响应体有内容, 状态码为200, 无报错)
 */
@property (nonatomic, assign) BOOL isValid;

/**
 响应报错
 */
@property (nonatomic, strong) NSError *error;

/**
 响应耗时(单位是毫秒)
 */
@property (nonatomic, assign) NSInteger spendTime;


/**
 记录产生的时间戳(单位是秒)
 */
@property (nonatomic, assign) NSUInteger requestTime;

/**
 请求地址
 */
@property (nonatomic, copy) NSString *requestUrl;

/**
 请求方式
 */
@property (nonatomic, copy) NSString *requestMethod;

/**
 客户端耗时(单位是毫秒)
 */
@property (nonatomic, assign) NSUInteger clientWasteTime NS_AVAILABLE_IOS(10_0);

/**
 网络请求总时间(单位是毫秒)
 */
@property (nonatomic, assign) NSUInteger totalTime NS_AVAILABLE_IOS(10_0);

/**
 DNS解析时间(单位是毫秒)
 */
@property (nonatomic, assign) NSUInteger dnsTime NS_AVAILABLE_IOS(10_0);

/**
 ssl时间(单位是毫秒)
 */
@property (nonatomic, assign) NSUInteger sslTime NS_AVAILABLE_IOS(10_0);

/**
 tcp时间(单位是毫秒)
 */
@property (nonatomic, assign) NSUInteger tcpTime NS_AVAILABLE_IOS(10_0);

/**
 首包时间(单位是毫秒)
 */
@property (nonatomic, assign) NSUInteger firstPacketTime NS_AVAILABLE_IOS(10_0);


@end

NS_ASSUME_NONNULL_END
