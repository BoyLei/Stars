//
//  GSSDK.h
//  GSSDK
//
//  Created by yoka on 2024/4/25.
//

#import <Foundation/Foundation.h>
#import <GSSDK/GSConstant.h>

NS_ASSUME_NONNULL_BEGIN

// 回调block类型，模块名称、方法名称、调用结果
typedef void(^GSCallback)(NSString *moduleName, NSString *funcName, NSString *result);

@interface GSSDK : NSObject

/**
 *  初始化接口
 *  @param data 默认可传nil
 *  @param callbackBlock  回调block，invoke_xxx方法也通过此block回调
 */
+ (void)initWithData:(nullable NSDictionary *)data callback:(GSCallback)callbackBlock;

/**
 *  void 型方法接口。
 *
 *  @param moduleName   模块名称
 *  @param funcName     方法名称
 *  @param parameters   调用参数，该参数请按文档说明传入
 */
+ (void)call:(NSString *)moduleName funcName:(NSString *)funcName parameters:(nullable NSDictionary *)parameters;

/**
 *  int型方法接口。
 *
 *  @param moduleName   模块名称
 *  @param funcName     方法名称
 *  @param parameters   调用参数，该参数请按文档说明传入
 */
+ (BOOL)callBool:(NSString *)moduleName funcName:(NSString *)funcName parameters:(nullable NSDictionary *)parameters;

/**
 *  String型方法接口。
 *
 *  @param moduleName   模块名称
 *  @param funcName     方法名称
 *  @param parameters   调用参数，该参数请按文档说明传入
 */
+ (NSString *)callString:(NSString *)moduleName funcName:(NSString *)funcName parameters:(nullable NSDictionary *)parameters;

/**
 *  int型方法接口。
 *
 *  @param moduleName   模块名称
 *  @param funcName     方法名称
 *  @param parameters   调用参数，该参数请按文档说明传入
 */
+ (int)callInt:(NSString *)moduleName funcName:(NSString *)funcName parameters:(nullable NSDictionary *)parameters;


@end

NS_ASSUME_NONNULL_END
