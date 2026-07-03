//
//  GSConstant.h
//  GS
//
//  Created by VisonCui on 2024/04/29.
//

#ifndef GSConstant_h
#define GSConstant_h

#pragma mark - Init
static NSString *const GS_FUNC_INIT            = @"init";// 初始化函数，每个模块都有初始化函数

#pragma mark - Platform Keys
// 登录支付等基本功能
static NSString *const GS_MODULE_PLATFORM      = @"platform";    // 登录支付模块名称
static NSString *const GS_FUNC_LOGIN           = @"login";       // 登录方法
static NSString *const GS_FUNC_LOGOUT          = @"logout";      // 注销方法
static NSString *const GS_FUNC_SWITCH_ACCOUNT  = @"switchAccount";//切换账号
static NSString *const GS_FUNC_BIND            = @"bindAccount"; // 绑定
static NSString *const GS_FUNC_PAY             = @"pay";         // 支付方法
static NSString *const GS_FUNC_QUERY_PRODUCT_DETAILS = @"queryProductDetails"; // 查询商品

// 上报进入游戏的数据
static NSString *const GS_FUNC_ENTER_GAME      = @"enterGame";      // 进入游戏方法

#pragma mark - 选服Keys
static NSString *const GS_MODULE_SERVICE                = @"service";      // 选服模块
static NSString *const GS_FUNC_LAST_SERVER_INFO         = @"lastServerInfo"; // 最后一次登录区服
static NSString *const GS_FUNC_DEFAULT_SERVER           = @"defaultServer"; // 默认区服
static NSString *const GS_FUNC_RECOMMENT_SERVERLIST     = @"recommendServerList"; // 推荐区服列表
static NSString *const GS_FUNC_AREAID_LIST              = @"areaIDList"; // 区服组
static NSString *const GS_FUNC_SERVER_LIST              = @"serverList"; // 区服列表

#pragma mark - analytics Keys
static NSString *const GS_MODULE_ANALYTICS         = @"analytics";       // analytics模块名称
static NSString *const GS_FUNC_TRACK         = @"track";       // track 打点方法


#pragma mark - Tools Keys
static NSString *const GS_MODULE_TOOLS         = @"tool";       // 工具模块名称
static NSString *const GS_FUNC_GET_DEVICE_INFO         = @"getDeviceInfo"; // 获取设备信息
static NSString *const GS_FUNC_GET_CHANNEL_INFO         = @"getChannelInfo"; // 获取Channel相关信息

#pragma mark - Help Keys
static NSString *const GS_MODULE_HELP         = @"help";       // 客服模块名称

#pragma mark - privacy Keys
static NSString *const GS_MODULE_PRIVACY         = @"privacy";             // 隐私模块名称
static NSString *const GS_FUNC_OPENAGREE         = @"openAgreement";       // 调用一次打开异常
static NSString *const GS_FUNC_SHOWAGREE         = @"showAgreement";       // 只会打开一次（除非有更新）
static NSString *const GS_FUNC_OPENAGREEURL      = @"openAgreementUrl";    // 打开具体的协议

#pragma mark - ad Keys
static NSString *const GS_MODULE_AD            = @"ad";                     // 广告
static NSString *const GS_FUNC_AD_SHOW         = @"show";                   // 方法名称
static NSString *const GS_FUNC_AD_PRELOAD      = @"preload";                // 方法名称

#pragma mark - Error Code

static int GSCODE_SUCCESS = 0;   // 操作成功
static int GSCODE_LOGIN_FAILE = 10005;   // 登录失败
static int GSCODE_LOGIN_CANCEL = 10006;   // 登录取消
static int GSCODE_PAY_FAILED = 10017;   //  支付失败
static int GSCODE_PAY_CANCEL = 10018;   //  支付取消
static int GSCODE_FAILURE = -1;   // 操作失败

#endif /* GSConstant_h */
