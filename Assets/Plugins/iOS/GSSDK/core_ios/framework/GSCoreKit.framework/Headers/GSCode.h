//
//  GsCode.h
//  GSSDK
//
//  Created by sen on 2021/12/14.
//

#ifndef GSCode_h
#define GSCode_h

/// 请勿使用，后期会移除

/**
 * 通用
 */
#define STATUS_SUCCESS                  0
#define STATUS_FAILED                   1
#define STATUS_ERROR                    2
#define STATUS_CANCEL                   3
#define STATUS_PROCESS                  4
#define STATUS__UNKNOWN                 -1

/**
 * 网络状态
 */
#define NETWORK_TYPE_RESTRICTED         101         // 网络受限
#define NETWORK_TYPE_NOTRESTRICTED      102         // 网络不受限

/**
 * 检查版本
 */
#define CHECK_VERSION_TYPE_NEW          1200        //检查到有版本更新
#define CHECK_VERSION_TYPE_WITHOUT_NEW  1201        //检查到没有版本更新
#define CHECK_VERSION_TYPE_WITHOUT      1202        //没有版本更新接口


#pragma mark 具体成功与失败一般是判定是否为0作为标识
//#define LOGIN_SUCCESS                       0         //登陆成功
//#define LOGIN_FAILED                        1         //登陆失败
//#define PAY_SUCCESS                         2         //支付成功
//#define PAY_FAILED                          3         //支付失败
//#define LOGOUT_SUCCESS                      4         //注销成功
//#define LOGOUT_FAILED                       5         //注销失败
//#define SWITCH_SUCCESS                      6         //切换成功
//#define SWITCH_FAILED                       7         //切换失败
//#define EXIT_SUCCESS                        8         //退出游戏成功
//#define EXIT_FAILED                         9         //退出游戏失败
//#define CHECK_NET_SUCCESS                   10        //检查网络环境成功
//#define CHECK_NET_FAILED                    11        //检查网络环境失败
//#define SHARE_SUCCESS                       12        //分享成功
//#define SHARE_FAILED                        13        //分享失败
//#define SPECIAL_SUCCESS                     14        //触发特殊接口成功
//#define SPECIAL_FAILED                      15        //触发特殊接口失败
//#define TRANSLATE_SUCCESS                   16        //翻译成功
//#define TRANSLATE_FAILED                    17        //翻译失败
//#define INIT_SUCCESS                        18        //初始化成功
//#define INIT_FAILED                         19        //初始化成功
//#define QUERY_REAL_NAME_SCUCESS             20        //查询实名认证信息成功
//#define QUERY_REAL_NAME_FAILED              21        //查询实名认证信息失败
//#define INIT_UNINIT                         22        //没有初始化
//#define LOGIN_UNLOGIN                       23        //没有登录
//#define NO_NETWORK                          24        //没有网络


#endif /* GsCode_h */
