//
//  GSHttpClient.h
//  TestDemo
//
//  Created by YOKA on 2022/8/23.
//

#import <Foundation/Foundation.h>
#import "GSHttpResponse.h"

NS_ASSUME_NONNULL_BEGIN

typedef void(^GSHttpResHandle)(GSHttpResponse *res);

@interface GSHttpClient : NSObject

@property (nonatomic, copy) NSString *baseURL;
@property (nonatomic, assign) NSTimeInterval timeoutInterval;
@property (nonatomic, copy) NSDictionary<NSString *, NSString *> *allHTTPHeaderFields;
@property (nonatomic, assign) BOOL completedInMainThread; // 默认开启主线程回调
@property (nonatomic, assign) BOOL collectingMetrics; // 默认不开启网络监听收集

- (instancetype)init NS_UNAVAILABLE;
- (instancetype)initWithBaseURL:(NSString *)baseURL;

+ (instancetype)client;
+ (instancetype)clientWithBaseURL:(nullable NSString *)baseURL;

- (void)asyncGETRequest:(NSString *)url body:(nullable id)body completion:(GSHttpResHandle)completion;
- (void)asyncPOSTRequest:(NSString *)url body:(nullable id)body completion:(GSHttpResHandle)completion;

- (GSHttpResponse *)syncGETRequest:(NSString *)url body:(nullable id)body;
- (GSHttpResponse *)syncPOSTRequest:(NSString *)url body:(nullable id)body;

@end

NS_ASSUME_NONNULL_END
