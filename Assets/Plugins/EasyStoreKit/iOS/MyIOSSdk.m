//  MyIOSSdk.m  
  
#import "MyIOSSdk.h"  
#import "PKS_Utility.h"
  
//这里引用SDK的头文件  
//#import <YYJPlatform/YYJPlatform.h>


#if defined(__cplusplus)  
extern "C"{  
#endif  
    extern void UnitySendMessage(const char *, const char *, const char *);
    extern NSString* _CreateNSString (const char* string);  
#if defined(__cplusplus)  
}  
#endif  
  
//*****************************************************************************  
  
@implementation MyIOSSdk  
  
//**********************  
//message tools  
  
+ (void)sendU3dMessage:(NSString *)messageName param:(NSDictionary *)dict  
{  
    NSString *param = @"";  
    if ( nil != dict ) {  
        for (NSString *key in dict)  
        {  
            if ([param length] == 0)  
            {  
                param = [param stringByAppendingFormat:@"%@=%@", key, [dict valueForKey:key]];  
            }  
            else  
            {  
                param = [param stringByAppendingFormat:@"&%@=%@", key, [dict valueForKey:key]];  
            }  
        }  
    }  
    UnitySendMessage("_MIOSSDK_", [messageName UTF8String], [param UTF8String]);  
}  
  
//**********************  
//SDK fun  
  
//初始化SDK  
-(void)SDKInit:(NSString *)appId appKey:(NSString *)appKey
{
    /*
     //初始化完成之后，会有初始化完成通知（ YYJPlatformInitDidFinishedNotification ）开发者在该回调方法中做登录操作
     //该通知应注册在初始化之前
    
    //添加一个初始化通知观察者，初始化结束后，登录等操作务必在收到该通知后调用
    //[[NSNotificationCenter defaultCenter]addObserver:self selector:@selector(yyjPlatformInitFinished) name:YYJPlatformInitDidFinishedNotification object:nil];
    
    // 添加一个支付成功通知观察者
    
   // [[NSNotificationCenter defaultCenter]addObserver:self selector:@selector(yyjPaySuccessful) name:YYJPlatformPaySuccessfulNotification object:nil];
    
    // 添加一个支付失败通知观察者
    
  //  [[NSNotificationCenter defaultCenter]addObserver:self selector:@selector(yyjPayFail) name:YYJPlatformPayFailNotification object:nil];
    

    //添加一个登录成功通知观察者，调用悬浮框等操作务必在收到该通知后调用
  //   [[NSNotificationCenter defaultCenter]addObserver:self selector:@selector(yyjPlatformLogin) name:YYJPlatformLoginNotification object:nil];
    
    //添加一个注销成功通知观察者
  //  [[NSNotificationCenter defaultCenter]addObserver:self selector:@selector(yyjPlatformLogout) name:YYJPlatformLogoutNotification object:nil];
    
    // SDK初始化，务必放在其他接口调用之前
 //   [[YYJPlatform defaultPlatform]initializeWithAppId:appId appKey:appKey appScheme:@"zdct"];
    // 输出日志
 //   [[YYJPlatform defaultPlatform]yyjIsShowLog:YES];
     
     */
}  
// 调用登录方法
- (void)login {
    //[[YYJPlatform defaultPlatform]YYJUserLogin];
}
// 注销点击事件
- (void)logout {
    //[[YYJPlatform defaultPlatform]YYJUserLogout];
}
// 切换登录点击事件
- (void)changeLogin {
    //[[YYJPlatform defaultPlatform]YYJUserChangeLogin];
}

//充值点击事件
- (void)Pay:(NSString *)rmb productId:(NSString *)productId productName:(NSString *)name charId:(NSString *)charid serverId:(NSString *)serverid expandInfo:(NSString *)info cporderId:(NSString *)cporderid
{
    //NSDate* dat = [NSDate dateWithTimeIntervalSinceNow:0];
    //NSTimeInterval a=[dat timeIntervalSince1970];
    //NSString *timeString = [NSString stringWithFormat:@"%.f%u", a,arc4random() % 100];
    // 这里的 timeString 只是为了模拟 订单ID ,实际中 将游戏商订单ID传给cporderId
    // productID 需要自己去苹果后台配置 (内购用, 不需要可不传) Demo中 苹果支付不可用
   //  [[YYJPlatform defaultPlatform]YYJPayWithMoney:rmb productID:productId productName:name charId:charid serverId:serverid expandInfo:info cporderId:cporderid];
}
 
// 初始化完成后调用的通知方法
- (void)yyjPlatformInitFinished
{
	//[MyIOSSdk sendU3dMessage:@"onInitSucc" param:nil];
}

// 登录成功后调用的通知方法
- (void)yyjPlatformLogin
{ 
    //调用悬浮框,开发者继续游戏逻辑
   // [[YYJPlatform defaultPlatform]showFloatWindow];
    
	//NSDictionary *dict = @{@"sid":[[YYJPlatform defaultPlatform]YYJUserUID],@"token":[[YYJPlatform defaultPlatform]YYJToken]};
	//[MyIOSSdk sendU3dMessage:@"onLoginSucc" param:dict];
    
}

- (void)yyjPlatformLogout
{
    NSLog(@"** YYJSDK 注销成功 **");
	//[MyIOSSdk sendU3dMessage:@"onLogoutSucc" param:nil];
}
- (void)yyjPaySuccessful
{
    NSLog(@"** YYJSDK 支付成功 **");
	//[MyIOSSdk sendU3dMessage:@"onCreateOrderSucc" param:nil];
}
- (void)yyjPayFail
{
    NSLog(@"** YYJSDK 支付失败 **");
	//[MyIOSSdk sendU3dMessage:@"onPayUserExit" param:nil];
}


- (void)submitRole:(NSString *)serverid serverName:(NSString *)serverName charId:(NSString *)charId roleName:(NSString *)roleName roleLevel:(NSString *)roleLevel time:(NSString *)time
{
    // 提交角色明细的方法  以下参数均为虚假的
     //[[YYJPlatform defaultPlatform]submitRoleInfoWithServerid:serverid andServerName:serverName andCharid:charId andRoleName:roleName andRolelevel:roleLevel andRoleTime:time];
}
// 配置支付回调接口
- (BOOL)application:(UIApplication *)application
            openURL:(NSURL *)url
  sourceApplication:(NSString *)sourceApplication
         annotation:(id)annotation {
    
    //[[YYJPlatform defaultPlatform]processOrderWithPaymentResult:url andAapplication:application];
    
    
    return YES;
}

// NOTE: 9.0以后使用新API接口
- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary<NSString*, id> *)options
{
    
    //[[YYJPlatform defaultPlatform]processOrderWithPaymentResult:url andAapplication:app];
    
    
    return YES;
}
// 微信支付回调
- (void)applicationWillEnterForeground:(UIApplication *)application {
    
    //[[YYJPlatform defaultPlatform]yyjSDKPayWillEnterForeground];
    
}

//获取用户ID  
-(NSString*)SDKGetUserID  
{  
    //return [[YYJPlatform defaultPlatform]YYJUserUID];
}  
  
//**********************  
//call back fun  
  
//初始化更新回调  
- (void)SNSInitResult:(NSNotification *)notify  
{  
    //[MyIOSSdk sendU3dMessage:@"onInitSucc" param:nil];
}  
  
@end  
  
  
//*****************************************************************************  
  
#if defined(__cplusplus)  
extern "C"{  
#endif  
      
    //字符串转化的工具函数  
      
    NSString* _CreateNSString (const char* string)  
    {  
        if (string)  
            return [NSString stringWithUTF8String: string];  
        else  
            return [NSString stringWithUTF8String: ""];  
    }  
      
    char* _MakeStringCopy( const char* string)  
    {  
        if (NULL == string) {  
            return NULL;  
        }  
        char* res = (char*)malloc(strlen(string)+1);  
        strcpy(res, string);  
        return res;  
    }  
      
    static MyIOSSdk *mySDK;  
	
	void __SetDebugMode()
	{
	}
  
    //供u3d调用的c函数  
    
    void __InitSDK(const char *appid,const char *appkey)  
    {  
        if(mySDK==NULL)  
        {  
            mySDK = [[MyIOSSdk alloc]init];  
        } 
        [mySDK SDKInit:[PKS_Utility CreateNSString:appid] appKey:[PKS_Utility CreateNSString:appkey]];  
    }  
	void __Login()
	{
		[mySDK login]; 
	}
	void __LogOut()
	{
		[mySDK logout]; 
	}
	void __Pay(char* aOrderId, char* aProductId, char* aProductName, char* aPrice, char* aNumber, char* sign, char* serverId, char* charId)
	{
		[mySDK Pay:[PKS_Utility CreateNSString:aPrice] productId:[PKS_Utility CreateNSString:aProductId] productName:[PKS_Utility CreateNSString:aProductName] charId:[PKS_Utility CreateNSString:charId] serverId:[PKS_Utility CreateNSString:serverId] expandInfo:[PKS_Utility CreateNSString:sign] cporderId:[PKS_Utility CreateNSString:aOrderId]];
	}
	void __SubmitRoleData(char* serverid, char* serverName, char* charId, char* roleName, char* roleLevel, char* time)
	{
		//[mySDK submitRole]; 
		[mySDK submitRole:[PKS_Utility CreateNSString:serverid] serverName:[PKS_Utility CreateNSString:serverName] charId:[PKS_Utility CreateNSString:charId] roleName:[PKS_Utility CreateNSString:roleName] roleLevel:[PKS_Utility CreateNSString:roleLevel] time:[PKS_Utility CreateNSString:time]];
	}
	
    void __ShowFloatWindow(BOOL b)
    {
        if(b)
        {
            //[[YYJPlatform defaultPlatform]showFloatWindow];
        }
        else
        {
            //[[YYJPlatform defaultPlatform]dismissFloatWindow];
        }
        
    }
    //注意这个函数是返回字符串  
    const char* __PlatformGetUin()  
    {  
        if(mySDK==NULL)  
        {  
            mySDK = [[MyIOSSdk alloc]init];  
        }  
        return _MakeStringCopy([[mySDK SDKGetUserID] UTF8String]);  
    }  
      
#if defined(__cplusplus)  
}  
#endif  
