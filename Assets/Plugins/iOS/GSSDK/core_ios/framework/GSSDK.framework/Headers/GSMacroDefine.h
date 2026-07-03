//
//  GSMacroDefine.h
//  GS
//
//  Created by VisonCui on 2024/4/29.
//

#ifndef GSMacroDefine_h
#define GSMacroDefine_h

// 主线程运行
#define dispatch_async_main(block) [NSThread currentThread]==[NSThread mainThread]?block():dispatch_async(dispatch_get_main_queue(),block);

// 捕捉异常
#if DEBUG
#define catch_exception_start
#define catch_exception_end
#else
#define catch_exception_start @try {

#define catch_exception_end }@catch (NSException *exception) {\
NSString *log = [NSString  stringWithFormat:@"%s,捕捉到异常\n*** Terminating app due to uncaught exception '%@',reson: %@,\n*** First throw call stack:\n %@", __PRETTY_FUNCTION__,exception.name,exception.reason,exception.callStackSymbols];\
NSLog(@"%@", log);}

#define catch_exception_end_handle(stuff) }@catch (NSException *exception) {\
NSString *log = [NSString  stringWithFormat:@"%s,捕捉到异常\n*** Terminating app due to uncaught exception '%@',reson: %@,\n*** First throw call stack:\n %@", __PRETTY_FUNCTION__,exception.name,exception.reason,exception.callStackSymbols];\
NSLog(@"%@", log);\
stuff\
}
#endif

// 崩溃
#define GSAssert(desc) dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), \
^{[[NSAssertionHandler currentHandler]handleFailureInMethod:_cmd object:self file:[NSString stringWithUTF8String:__FILE__] lineNumber:__LINE__ description:desc];});

#define NSReplaceNil(Value,newValue) ((Value==nil||[Value isEqual:[NSNull null]])?newValue:Value)

#endif /* GSMacroDefine_h */
