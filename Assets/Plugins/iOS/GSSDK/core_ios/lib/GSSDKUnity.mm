//
//  GSSDKUnity.m
//  GSSDKDemo
//
//  Created by YOKA on 2023/6/16.
//


#import <Foundation/Foundation.h>
#import <objc/runtime.h>
#import <objc/message.h>

typedef void(^__GSCallback)(NSString *moduleName, NSString *funcName, NSString *result);

typedef void (*GSNativeStringHandler)(const char* moduleName, const char* funcName, const char* result);
GSNativeStringHandler gs_callback = NULL;
static char* __GSStrcpy(NSString *str);
static NSString *__GSStringByChar(const char* ch);
static NSString *__GSCharByString(NSString *string);
static NSDictionary * __GSCallParametersConvertByJSONString(const char* parameters);


#define GSSDKClass NSClassFromString(@"GSSDK")

#define SelectorCheckWithResult(selector) if (!check_selector(selector)) { return nil; }
#define SelectorCheckWithNoneResult(selector) if (!check_selector(selector)) { return; }

@interface GSSDKUnity: NSObject
@property (nonatomic, strong) __GSCallback callback;
@end

@implementation GSSDKUnity

static GSSDKUnity *_instance = nil;

+ (instancetype)sharedInstance {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _instance = [[GSSDKUnity alloc] init];
        [_instance setupCallback];
    });
    return _instance;
}

- (void)setupCallback
{
    self.callback = ^(NSString *moduleName, NSString *funcName, NSString *result) {
        if (gs_callback) {
            gs_callback(__GSStrcpy(moduleName), __GSStrcpy(funcName), __GSStrcpy(result));
        } else {
            NSLog(@"Please call `GSUnityRegisterNativeCallback` to receive message");
        }
    };
}

@end


#ifdef __cplusplus
extern "C" {
#endif

id gssdk_reflection_with_result(SEL selector, const char* moduleName, const char* funcName, const char* parameters);
BOOL check_selector(SEL selector);

#pragma mark
BOOL check_selector(SEL selector)
{
    if (!GSSDKClass) {
        NSLog(@"[GSSDKUnity] Please import GSSDK.framework and set -ObjC");
        return NO;
    }
    if (![GSSDKClass respondsToSelector:selector]) {
        NSLog(@"[GSSDKUnity] Unknown selector [%@]", NSStringFromSelector(selector));
        return NO;
    }
    return YES;
}
    

void GSUnityRegisterNativeCallback(GSNativeStringHandler callback)
{
    gs_callback = callback;
}

void GSInit(const char* data)
{
    SEL selector = NSSelectorFromString(@"initWithData:callback:");
    SelectorCheckWithNoneResult(selector);
    ((void(*)(Class, SEL, id, id))objc_msgSend)(GSSDKClass,
                                                selector,
                                                __GSCallParametersConvertByJSONString(data),
                                                [GSSDKUnity sharedInstance].callback
                                                );
}

void GSCall(const char* moduleName, const char* funcName, const char* parameters)
{
//    [GSSDK call:__GSStringByChar(moduleName)
//       funcName:__GSStringByChar(funcName)
//     parameters:__GSCallParametersConvertByJSONString(parameters)];
    SEL selector = NSSelectorFromString(@"call:funcName:parameters:");
    SelectorCheckWithNoneResult(selector);
    ((void(*)(Class, SEL,id,id,id))objc_msgSend)(GSSDKClass,
                                                 selector,
                                                 __GSStringByChar(moduleName),
                                                 __GSStringByChar(funcName),
                                                 __GSCallParametersConvertByJSONString(parameters)
                                                 );
}

int GSCallInt(const char* moduleName, const char* funcName, const char* parameters)
{
//    return [GSSDK callInt:__GSStringByChar(moduleName)
//                 funcName:__GSStringByChar(funcName)
//               parameters:__GSCallParametersConvertByJSONString(parameters)];
    NSNumber *result = gssdk_reflection_with_result(
                                                    NSSelectorFromString(@"callInt:funcName:parameters:"),
                                                    moduleName,
                                                    funcName,
                                                    parameters
                                                    );
    return result.intValue;
}


bool GSCallBool(const char* moduleName, const char* funcName, const char* parameters)
{
//    return [GSSDK callBool:__GSStringByChar(moduleName)
//                  funcName:__GSStringByChar(funcName)
//                parameters:__GSCallParametersConvertByJSONString(parameters)];
    
    NSNumber *result = gssdk_reflection_with_result(
                                                    NSSelectorFromString(@"callBool:funcName:parameters:"),
                                                    moduleName,
                                                    funcName,
                                                    parameters
                                                    );
    return result.boolValue;
}


char* GSCallString(const char* moduleName, const char* funcName, const char* parameters)
{
    NSString *result = gssdk_reflection_with_result(
                                                    NSSelectorFromString(@"callString:funcName:parameters:"),
                                                    moduleName,
                                                    funcName,
                                                    parameters
                                                    );
//    NSString *result = [GSSDK callString:__GSStringByChar(moduleName)
//                                funcName:__GSStringByChar(funcName)
//                              parameters:__GSCallParametersConvertByJSONString(parameters)];
    return __GSStrcpy(result);
}

    
id gssdk_reflection_with_result(SEL selector, const char* moduleName, const char* funcName, const char* parameters)
{
    SelectorCheckWithResult(selector);
    return ((id(*)(Class, SEL,id,id,id))objc_msgSend)(GSSDKClass,
                                                      selector,
                                                      __GSStringByChar(moduleName),
                                                      __GSStringByChar(funcName),
                                                      __GSCallParametersConvertByJSONString(parameters)
                                                      );
}


static NSString *__GSStringByChar(const char* ch)
{
    NSString *string = nil;
    if (ch != NULL) {
        string = [NSString stringWithUTF8String:ch];
    }
    return string;
}


static NSDictionary * __GSCallParametersConvertByJSONString(const char* parameters)
{
    NSString *parametersStr = __GSStringByChar(parameters);
    NSDictionary *parametersDic = nil;
    if (parametersStr && [parametersStr isKindOfClass:[NSString class]]) {
        NSData *parametersData = [parametersStr dataUsingEncoding:NSUTF8StringEncoding];
        parametersDic = [NSJSONSerialization JSONObjectWithData:parametersData options:NSJSONReadingMutableContainers error:nil];
    }
    return parametersDic;
}


static char* __GSStrcpy(NSString *str) {
    if (!str || ![str isKindOfClass:[NSString class]]) {
        return NULL;
    }
    
    const char *src = [str UTF8String];
    
    if (src == NULL) {
        return NULL;
    }
    
    char *dst = (char *)malloc(strlen(src) + 1);
    strcpy(dst, src);
    return dst;
}

#ifdef __cplusplus
}
#endif
