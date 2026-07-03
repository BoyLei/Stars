//
//  GSConvertor.h
//  GSCoreKit
//
//  Created by YOKA on 2023/6/19.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GSConvertor : NSObject

+ (NSString *)dataToString:(NSData *)data;

+ (id)dataToJSONObject:(NSData *)data;

+ (NSString *)JSONObjectToString:(id)object;

+ (id)stringToJSONObject:(NSString *)string;

@end

NS_ASSUME_NONNULL_END
