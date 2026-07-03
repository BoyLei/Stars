//
//  YKHWFileManager.h
//  YKHWMobileSDK
//
//  Created by yoka on 2021/12/9.
//  Copyright © 2021 Boson. All rights reserved.
//

#import <Foundation/Foundation.h>

#define kPB_CENTER_GLOBAL_CONFIG  @"PBUserCenterGlobalConfig"
#define kPB_CENTER_USER_INFO_MODEL  @"PBUserCenterUserInfoModel"
#define kPB_ACCOUNT_MANAGER_INFO_MODEL  @"PBAccountManagerUserInfoModel"

@interface GSFileManager : NSObject

#pragma mark - 本地文件处理(存、读、删、改)
/**
 直接将数据写入”来电数据“文件
 
 @param dataSource 数据信息
 */
+ (void)setDataToDocument:(id)dataSource;

/**
 保存数组数据到文件
 @param array 数组数据
 @param fileName 文件名
 */
+ (void)setArray:(NSArray *)array withFileName:(NSString *)fileName;

/**
 从文件获取数组数据
 @param fileName 文件名
 @return 数组数据
 */
+ (NSArray *)getArrayWithFileName:(NSString *)fileName;

/**
 保存字典数据到文件
 @param dic 字典数据
 @param fileName 文件名
 */
+ (void)setDictionary:(NSDictionary *)dic withFileName:(NSString *)fileName;

/**
 从文件获取字典数据
 @param fileName 文件名
 @return 字典数据
 */
+ (NSDictionary *)getDictionaryWithFileName:(NSString *)fileName;

/**
 是否成功移除了该文件
 @param fileName 文件名
 @return 是否成功：YES:成功   NO:失败
 */
+ (BOOL)isSuccessedRemoveFileAtPath:(NSString *)fileName;

#pragma mark - 缓存数据处理 (存、读、删、改)
/**
 是否成功创建缓存文件
 @param fileName 创建的缓存文件名
 @return 是否成功：YES:成功   NO:失败
 */
+ (BOOL)isSuccessedCreateCacheFileAtPath:(NSString *)fileName;

/**
 移除缓存
 @param folder 文件/文件夹
 */
+ (void)removeCachePath:(NSString *)folder;

/**
 NSUserDefault 存储对象
 
 @param value 对象值
 @param key key
 */
+ (void)saveValue:(id)value key:(NSString *)key;

/**
 NSUserDefault 取值
 
 @param key key
 @return value 对象值
 */
+ (id)loadValueWithKey:(NSString *)key;

/**
 NSUserDefault 存储BOOL值
 
 @param value BOOL值
 @param key key
 */
+ (void)saveBoolValue:(BOOL)value key:(NSString *)key;

/**
 NSUserDefault 取值
 
 @param key key
 @return BOOL 值
 */
+ (BOOL)loadBoolValueWithKey:(NSString *)key;

/**
 NSUserDefaults 移除值
 
 @param key key
 */
+ (void)removeDataObjectForKey:(NSString *)key;

@end
