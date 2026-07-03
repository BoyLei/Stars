//
//  GSConstants.h
//  GS
//
//  Created by yoka on 2022/1/29.
//  Copyright © 2022 Boson. All rights reserved.
//

#ifndef GSEnum_h
#define GSEnum_h

typedef NS_ENUM(NSInteger, GSRunMode) {
    GSRunModeProduction = 0,
    GSRunModeSandbox = 1
};

typedef NS_ENUM(NSInteger, GSEvent) {
    GSEventLogin = 0,
    GSEventLogout = 1,
    GSEventCreateRole = 2,
    GSEventExitGame = 3,
    GSEventLevelup = 4,
    GSEventPurchase = 5,
    GSEventShare = 6,
    GSEventUpdate = 7,
    GSEventInvite = 8,
    GSEventSignUp = 9,
    GSEventAddToWishlist = 10,
    GSEventAddToCart = 11,
    GSEventBeginCheckout = 12,
    GSEventSpendVirtualCurrency = 13,
    GSEventSelectServer = 14,
    GSEventAppInstall = 15,
    GSEventAppOpen = 16,
    GSEventJoinGuild = 17,
    GSEventVipLevel = 18,
    GSEventFirstPayAward = 19,
    GSEventOnlineTime = 20,
    GSEventPassChapter = 21,
    GSEventTutorialBegin = 22,
    GSEventFrontFunnel = 23,
    GSEventCompletedRegistration = 24,
    GSEventUnlockedAchievement = 25
};

typedef NS_ENUM(NSInteger, GSPrivacyAuthorizationType) {
    GS_AUTHORIZATIONTYPE_WHEN_USE_LOCATION = 0,
    GS_AUTHORIZATIONTYPE_ALWAYS_LOCATION,
    GS_AUTHORIZATIONTYPE_PHOTO_LIBRARY,
    GS_AUTHORIZATIONTYPE_CAMERA,
    GS_AUTHORIZATIONTYPE_MICROPHONE,
    GS_AUTHORIZATIONTYPE_MOTION,
    GS_AUTHORIZATIONTYPE_SIRI,
    GS_AUTHORIZATIONTYPE_SPEECH_RECOGNITION,
    GS_AUTHORIZATIONTYPE_ADVERTISING_TRACKING
};

typedef NS_ENUM(NSInteger, GSCallbackType) {
    GSCallbackType_INIT = 0,
    GSCallbackType_LOGIN,
    GSCallbackType_LOGOUT,
    GSCallbackType_PAY,
    GSCallbackType_SHARE,
    GSCallbackType_BIND,
    GSCallbackType_ID,
    GSCallbackType_GENCITATION_CODE,
    GSCallbackType_LOGIN_CITATION_CODE,
    GSCallbackType_TRANSLATE_WITH_TEXT,
    GSCallbackType_PRIVACYAUTHORIZATION,
    GSCallbackType_UPDATE_VOUCHER_NUM,
    GSCallbackType_PAY_VOUCHER_INFO,
    GSCallbackType_CENTER_RECEIVE_GIFT,
    GSCallbackType_CENTER_INIT,
    GSCallbackType_CENTER_CLICK,
    GSCallbackType_PRIVACYONLY,
    GSCallbackType_PERIOD_CANCEL,
    GSCallbackType_COOL_PERIOD_DELETE_SUCCESS,
    GSCallbackType_DELETE_ACCOUNT,
    GSCallbackType_QUERY_PRODUCT_DETAILS,
    GSCallbackType_AD_FINISH_LOADING,
    GSCallbackType_AD,
    GSCallbackType_WEBVIEW,
    GSCallbackType_LAST_SERVER_INFO,
    GSCallbackType_AREAID_LIST,
    GSCallbackType_DEFAULT_SERVER,
    GSCallbackType_RECOMMEND_SERVER_LIST,
    GSCallbackType_SERVER_LIST
};

// Log level for Bugly Log
typedef NS_ENUM(NSUInteger, GSBuglyLogLevel) {
    GSBuglyLogLevelSilent  = 0,
    GSBuglyLogLevelError   = 1,
    GSBuglyLogLevelWarn    = 2,
    GSBuglyLogLevelInfo    = 3,
    GSBuglyLogLevelDebug   = 4,
    GSBuglyLogLevelVerbose = 5,
};

typedef NS_ENUM(NSInteger, GSTrack) {
    GSTrackActionBase = 1001, //行为日志基础类
    GSTrackActionAccount = 1002, //行为日志账号类
    GSTrackAccountPay = 1003, //行为日志支付类
    GSTrackTicket = 1004, //票据日志类
    GSTrackCrash = 1005, //崩溃报告
    GSTrackProtocol = 1006, //关键协议
    GSTrackRuntime = 1007, //运行时日志
    GSTrackWarning = 1008, //警告类
    GSTrackError = 1009, //错误类
};

typedef NS_ENUM(NSInteger,GSWebOrientationType){
    GSWebOrientationLandScape = 0,//横屏 默认横屏
    GSWebOrientationPortrait = 1  //竖屏
};

typedef NS_ENUM(NSInteger,GSDomesticFuneralType) {
    GSDomesticInit = 1,      //初始化sdk
    GSDomesticPrivacy = 2,//勾选用户协议和隐私条例1
    GSDomesticCGBegin = 3,//开始播放CG
    GSDomesticCGSkip = 4,//点击确认跳过
    GSDomesticCGEnd = 5,//CG播放结束
    GSDomesticBeginLocalFile = 6,//开始校验本地文件
    GSDomesticEndLocalFile = 7,//检验本地文件完成
    GSDomesticNoUpdateContent = 8,//无更新内容
    GSDomesticYesUpdateContent = 9,//有更新内容
    GSDomesticBeginCdn = 10,//开始获取CDN地址
    GSDomesticSucCdn = 11,//CDN地址获取成功
    GSDomesticTipSizeUpdate = 12,//提示更新内容大小
    GSDomesticUpdateFail = 13,//更新失败
    GSDomesticUpdateSuc = 14,//更新成功
    GSDomesticLoadSceneResource = 15,//加载场景资源
    GSDomesticLoadSceneResourceSuc = 16,//加载场景资源
    GSDomesticChkPrivacy_2 = 17,//勾选用户协议和隐私条例2
    GSDomesticClickLoginButton = 18,//点击开始登录
    GSDomesticShowMobileWindow = 19,//弹出手机号登录窗口
    GSDomesticShutMobileWindow = 20,//关闭手机登录窗口
    GSDomesticInputMobile = 21,//关闭手机登录窗口
    GSDomesticClickVerificationCode = 22,//点击获取验证码
    GSDomesticInputVerificationCode = 23,//输入手机验证码
    GSDomesticShowRealNameWindow = 24,//弹出游戏实名认证窗口
    GSDomesticShutRealNameWindow = 25,//关闭实名窗口
    GSDomesticInputRealname = 26,//输入实名信息
    GSDomesticRealnameVerificationFail = 27,//实名认证失败
    GSDomesticRealnameVerificationSuc = 28,//实名认证成功
    GSDomesticLoginFail = 29,//登录失败
    GSDomesticLoginSuc = 30,//登录成功
    GSDomesticShowAnnouncement = 31,//弹出公告
    GSDomesticShutAnnouncement = 32,//关闭公告
    GSDomesticClickEnterGame = 33,//选择区服ID/进入游戏
    
} ;
//海外游戏前置埋点类型
typedef NS_ENUM(NSInteger,GSOverseasFuneralType) {
    GSOverseasInit = 1,      //初始化sdk
    GSOverseasCGBegin = 2,//开始播放CG
    GSOverseasCGSkip = 3,//点击确认跳过
    GSOverseasCGEnd = 4,//CG播放结束
    GSOverseasBeginLocalFile = 5,//开始校验本地文件
    GSOverseasEndLocalFile = 6,//检验本地文件完成
    GSOverseasNoUpdateContent = 7,//无更新内容
    GSOverseasYesUpdateContent = 8,//有更新内容
    GSOverseasBeginCdn = 9,//开始获取CDN地址
    GSOverseasSucCdn = 10,//CDN地址获取成功
    GSOverseasTipSizeUpdate = 11,//提示更新内容大小
    GSOverseasUpdateFail = 12,//更新失败
    GSOverseasUpdateSuc = 13,//更新成功
    GSOverseasLoadSceneResource = 14,//加载场景资源
    GSOverseasLoadSceneResourceSuc = 15,//加载场景资源成功
    GSOverseasClickLoginButton = 16,//点击登录/游客登录
    GSOverseasLoginFail = 17,//登录失败
    GSOverseasLoginSuc = 18,//登录成功
    GSOverseasShowAnnouncement = 19,//弹出公告
    GSOverseasShutAnnouncement = 20,//关闭公告
    GSOverseasClickEnterGame = 21,//选择区服ID/进入游戏
} ;

#endif /* GSEnum_h */
