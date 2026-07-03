//
//  NSObject+GSUtil.h
//  GSCoreKit
//
//  Created by YOKA on 2023/1/3.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

#define ToSafeStr(str) (str != nil?[str gs_toValidString]:@"")

@interface NSObject (GSUtil)

//转成安全的数据
- (NSString *)gs_toValidString;

//有效的字符串定义为类型是字符串并且长度>1
- (BOOL)gs_isValidString;
//去除空格有效的字符串定义为类型是字符串并且长度>1
- (BOOL)gs_isValidStringByTrimmingCharactersInSet;

/*
 是否是全部包含数字的字符串
 */
- (BOOL)gs_isValidStringWithAllNumber;

//有效的数据定义为类型是数据并且长度>1
- (BOOL)gs_isValidArray;
//有效的数据定义为字典而且长度>1
- (BOOL)gs_isValidDictionary;

- (BOOL)gs_isValidNumber;

- (BOOL)gs_isValidData;

- (BOOL)gs_isNilOrNull;

+ (NSDictionary *)dicFromModelObject:(NSObject *)object;

+ (NSDictionary *)dictionaryWithJSONString:(NSString *)jsonString;

+ (NSString *)dictionaryConvertToJSONString:(NSDictionary *)dict;


@end

@interface NSDictionary (GSUtil)

- (id)gs_safeObjectForKey:(NSString *)key;

- (id)gs_safeKeyForValue:(id)value;

- (NSString *)gs_safeStringForKey:(NSString *)key;

- (NSArray *)gs_safeArrayForKey:(NSString *)key;

- (NSDictionary *)gs_safeDictionaryForKey:(NSString *)key;

- (BOOL)gs_safeBoolForKey:(NSString *)key;

- (NSInteger)gs_safeIntegerForkey:(NSString *)key;

@end

@interface NSMutableDictionary (GSUtil)

- (void)gs_safeSetObject:(id)object forKey:(id)key;

- (void)gs_safeRemoveObjectForKey:(id)key;

@end


@interface NSArray (GSUtil)

- (id)gs_safeObjectAtIndex:(NSInteger)index;

- (NSString *)gs_safeStringAtIndex:(NSInteger)index;

- (NSArray *)gs_safeArrayAtIndex:(NSInteger)index;

- (NSDictionary *)gs_safeDictionaryAtIndex:(NSInteger)index;

@end


@interface NSMutableArray (GSUtil)

-(void)gs_safeAddObject:(id)anObject;

-(void)gs_safeAddObjectSelectivity:(id)anObject;

-(void)gs_safeInsertObject:(id)anObject atIndex:(NSInteger)index;

-(void)gs_safeInsertObjectSelectivity:(id)anObject atIndex:(NSInteger)index;

-(void)gs_safeRemoveObject:(id)anObject;

-(void)gs_safeRemoveObjectAtIndex:(NSInteger)index;

@end

NS_ASSUME_NONNULL_END
