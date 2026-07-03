//
//  GSModuleManager.h
//  GSSDK
//
//  Created by yoka on 2024/4/29.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface GSModuleManager : NSObject

@property (nonatomic, strong) NSMutableArray *validModules;// 模块初始化成功后，将模块名加入

+ (GSModuleManager *)defaultManager;

// 模块初始化
+ (void)moduleInitWithData:(nullable NSDictionary *)data callback:(void(^)(NSString *moduleName, NSString *funcName, NSString *result))callbackBlock;

@end

NS_ASSUME_NONNULL_END
