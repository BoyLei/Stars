//
//  GSI18NUtil.h
//
//  Created by YOKA on 2022/9/15.
//

#import <Foundation/Foundation.h>

#define GSLocalizedString(key)   [GSI18NUtil localizedStringForKey:key]

NS_ASSUME_NONNULL_BEGIN

@interface GSI18NUtil : NSObject

+ (NSString *)localizedStringForKey:(NSString *)key;

+ (NSString *)localizedStringForKey:(NSString *)key table:(nullable NSString *)tableName;

+ (NSString *)localizedStringForKey:(NSString *)key table:(nullable NSString *)tableName bundleNamed:(NSString *)bundleName;

+ (NSString *)localizedStringForKey:(NSString *)key table:(nullable NSString *)tableName bundleNamed:(NSString *)bundleName bundleForClass:(Class)cla;

@end

NS_ASSUME_NONNULL_END
