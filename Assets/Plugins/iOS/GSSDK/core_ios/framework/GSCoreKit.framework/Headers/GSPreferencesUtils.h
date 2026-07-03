//
//  GSPreferencesUtils.h
//  GSCoreKit
//
//  Created by YOKA on 2023/2/20.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GSPreferencesUtils : NSObject

//获取UserDefaults
+ (id)userDefaultsForKey:(NSString *)key;
//设置UserDefaults
+ (void)setUserDefaults:(id)object forKey:(NSString *)key;
//获取UserDefaults Bool
+ (BOOL)userDefaultsBoolForKey:(NSString *)key;
//设置UserDefaults
+ (void)setUserDefaultsBool:(BOOL)isBool forKey:(NSString *)key;
// 清空某个key的值
+ (void)cleanUserDefaultsForKey:(NSString *)key;
+ (void)cleanAllUserDefaults;

@end

NS_ASSUME_NONNULL_END
