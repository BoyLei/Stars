//
//  GSHttpRequest.h
//  TestDemo
//
//  Created by YOKA on 2022/8/23.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GSHttpRequest : NSMutableURLRequest

+ (instancetype)GETRequestWithURL:(NSString *)URL body:(id)body;
+ (instancetype)POSTRequestWithURL:(NSString *)URL body:(id)body;

@end

NS_ASSUME_NONNULL_END
