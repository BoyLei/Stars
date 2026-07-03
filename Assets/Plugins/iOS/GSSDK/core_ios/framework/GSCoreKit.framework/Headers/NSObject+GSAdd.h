//
//  NSObject+GSAdd.h
//  GSCoreKit
//
//  Created by ritchie on 2024/5/30.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

NSString *gs_convertToStringBy(NSObject *);


@interface NSObject (GSCopy)
- (BOOL)gs_shallowCopy:(NSObject *)instance;
- (BOOL)gs_deepCopy:(NSObject *)instance;
@end


@interface NSObject (GSReflection)
- (NSString *)gs_className;
+ (NSString *)gs_className;
//父类名称
- (NSString *)gs_superClassName;
+ (NSString *)gs_superClassName;

//实例属性字典
- (NSDictionary *)gs_propertyDictionary;

//属性名称列表
- (NSArray*)gs_propertyKeys;
+ (NSArray *)gs_propertyKeys;

//属性详细信息列表
- (NSArray *)gs_propertiesInfo;
+ (NSArray *)gs_propertiesInfo;

//格式化后的属性列表
+ (NSArray *)gs_propertiesWithCodeFormat;

//方法列表
- (NSArray*)gs_methodList;
+ (NSArray*)gs_methodList;

- (NSArray*)gs_methodListInfo;

//创建并返回一个指向所有已注册类的指针列表
+ (NSArray *)gs_registedClassList;
//实例变量
+ (NSArray *)gs_instanceVariable;

//协议列表
- (NSDictionary *)gs_protocolList;
+ (NSDictionary *)gs_protocolList;


- (BOOL)gs_hasPropertyForKey:(NSString*)key;
- (BOOL)gs_hasIvarForKey:(NSString*)key;

@end

NS_ASSUME_NONNULL_END
