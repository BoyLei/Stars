//
//  YokaSdk.h
//  YokaSdk
//
//  Created by qingyun on 2024/4/3.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

/*! 开发环境
 *
 */
typedef enum  {
    YK_SERVERCONF_ENVIRONMENT_DEV     = 0,          /**< 测试   */
    YK_SERVERCONF_ENVIRONMENT_FORMAL  = 1           /**< 正式    */
}YK_SERVERCONF_ENVIRONMENT;

/*! 防沉迷
 *
 */
typedef enum  {
    YK_ANTIA_ADDICTION1     = 1,           /**< 未成年防沉迷在不可玩游戏的时间段提示 */
    YK_ANTIA_ADDICTION2     ,           /**< 未成年防沉迷在可玩时间段中途提示（还剩30分钟）    */
    YK_ANTIA_ADDICTION3     ,           /**< 未成年防沉迷可玩时间段结束    */
    YK_ANTIA_ADDICTION4     ,           /**< 支付超过一定金额提示    */
}YK_ANTIA_ADDICTION_TYPE;

@protocol  YokaSdkDelegate;


@interface YokaSdk : NSObject
@property (nonatomic, weak)id<YokaSdkDelegate>           delegate;               //代理
@property (nonatomic, copy)NSString                      *appid;                 //游卡appid
@property (nonatomic, assign)BOOL                        supportPadView;         //适配Pad 默认为iphone()
@property(nonatomic,assign)YK_SERVERCONF_ENVIRONMENT     ykEnvironment;          //设置环境
@property (nonatomic, copy)NSString                      *shanYanAppId;          //闪验APPID
@property(nonatomic,copy)NSString                        *type;                  //第三方登录配置
@property(nonatomic,copy)NSString                        *httpServerStr;         //修改域名格式如“dobest.cn”
@property(nonatomic,copy)NSString                        *areaId;                //区服ID
@property (nonatomic, copy)NSString                      *weixinAppid;           //第三方 微信 APPID
@property (nonatomic, copy)NSString                      *weixinUniversalLink;   //第三方 微信 universalLink
@property (nonatomic, copy)NSString                      *qqAppid;               //第三方 QQ APPID
@property (nonatomic, copy)NSString                      *qqUniversalLink;       //第三方 QQ universalLink
@property (nonatomic, copy)NSString                      *productUrl;            //第三方 苹果后台的URL
@property (nonatomic, copy)NSDictionary                  *userAgreeData;         //用户协议  默认读取配置

/**
 * 初始化配置
 */
-(void)initConfiguration;

/**
 * 获取单列实例
 */
+ (YokaSdk *)sharedInstance;

/**
 * 打开登录界面
 * callback    loginCallback
 */
-(void)login;

/**
 * 退出账号
 */
-(void)logoutAccount;

/**
 * 退出游戏--没有具体业务只有一个弹窗
 */
-(void)logoutGame;

/**
 * 打开防沉迷
 */
-(void)antiaAddiction:(YK_ANTIA_ADDICTION_TYPE)type;

/**
 * 绑定手机
 * callback    bindPhoneCallback
 */
-(void)bindPhone;

/**
 * 当前登录的用户是否有绑定手机号
 */
-(BOOL)isBindPhone;

/**
 * 游戏启动展示用户协议弹窗
 */
-(void)userAgreement;

/**
 * 获取当前登录的用户类型
 */
-(NSString*)getCurrentUserType;

/**
 * param URL
 * 第三方URL 设置
 */
-(void)ykhandleOpenURL :(NSURL*)url;



/**
 * 支付
 * param productId  itunesconnect上配置
 * param extension  服务器回传的自定义参数
 * callback payCallback
 */
-(void)pay:(NSString*)productId withExtension :(NSString*)extension withOrder:(NSString*)gameOrderId;

/**
 * 支付
 * param productId  itunesconnect上配置
 * param extension  服务器回传的自定义参数
 * param   扩展字段
 * callback payCallback
 */
-(void)pay:(NSString*)productId withExtension :(NSString*)extension withOrder:(NSString*)gameOrderId whtParam:(nullable NSDictionary*)param;
/**
 * param
 * 检查有没有未完成的订单。有的话就完成
 * 请在登录成功后调用此接口，或者在APP界面适当的位置做一个补单按钮（注意请不要和支付接口pay同时调用）
 * callback payCallback
 */
-(void)checkUndonePay;

@end

@protocol YokaSdkDelegate <NSObject>
@optional
/**
 * 支付回调
 */
-(void)payCallback:(NSDictionary*)data;
/**
 * 登录回调
 */
-(void)loginCallback:(NSDictionary*)data;
/**
 * 登出回调
 */
-(void)logoutAccountCallback:(NSDictionary*)data;

/**
 * 退出游戏回调
 */
-(void)logoutGameCallback:(NSDictionary*)data;

/**
 * 游戏启动展示用户协议弹窗回调
 */
-(void)userAgreementCallback:(NSDictionary*)data;
/**
 * 绑定手机回调
 */
-(void)bindPhoneCallback:(NSDictionary*)data;
/**
 * 行为日志
 */
-(void)actionLogCallback:(NSString*)actionStr;

/**
 * 行为日志（初始化配置、启动也用户协议）
 */
-(void)actionStartUpLogCallback:(NSString*)actionStr;

/**
 * 防沉迷回调 0未成年防沉迷在不可玩游戏的时间段提示  1未成年防沉迷在可玩时间段中途提示（还剩30分钟) 3未成年防沉迷可玩时间段结束
 */
-(void)antiaAddictionCallback:(NSDictionary*)data;

@end


NS_ASSUME_NONNULL_END
