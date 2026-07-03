//
//  GSBaseModule.h
//  GSSDK
//
//  Created by yoka on 2024/4/29.
//

#import <Foundation/Foundation.h>
#import <GSSDK/GSMacroDefine.h>
#import <GSCoreKit/GSCoreKit.h>
#import "GSConstant.h"
#import "GSModuleManager.h"
#import "GSAppConfig.h"

NS_ASSUME_NONNULL_BEGIN

// 模块回调函数
typedef void(^GSModuleCallback)(NSString *moduleName, NSString *funcName, NSString *result);

@interface GSBaseModule : NSObject

@property (nonatomic, readonly, copy) GSModuleCallback callbackBlock;

// 单例对象方法
+ (GSBaseModule *)sharedInstance;

+ (NSString *)moduleName;

// 模块初始化方法
+ (void)moduleInit:(GSModuleCallback)callbackBlock;

+ (void)callback:(NSString *)funcName code:(int)code msg:(NSString *)msg data:(nullable NSDictionary *)data;
// 回调方法，返回值YES为当前单例对象回调函数存在，NO标识当前单例对象回调不存在，无法完成回调
+ (BOOL)callback:(NSString *)funcName result:(id)result;

@end

NS_ASSUME_NONNULL_END
