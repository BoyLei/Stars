//
//  FacilityInformationUntil.h
//
//  Created by zls on 2019/5/10.
//  Copyright © 2019年 zls. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@interface GSDeviceInfor : NSObject

/// 设备指纹
@property (nonatomic, strong, readonly, class) NSString *c_oneID;
@property (nonatomic, strong, readonly, class) NSString *s_oneID;

// 1.获取设备名称 user-assigned device name (e.g. @"My iPhone").
+ (NSString *)getDeviceName;
// 获取手机名称 iPhone
+ (NSString *)getDeviceModel;
//获取os_version 16.2
+ (NSString *)getOs_version;
//获取操作系统
+ (NSString *)getDeviceSystemName;
//获取屏幕的分辨率
+ (CGSize)getPhoneSize;
// 获取内存大小
+ (NSString *)totalMemorySize;
//获取运营商
+ (NSString *)getCurrentOperator;
/** 获取当前网络 */
+ (NSString *)getCurrentNerWork;
//获取App版本信息
+ (NSString *)appBundleId;
+ (NSString *)appVersion;
+ (NSString *)appBuildVersion;
+ (NSString *)appName;
// 客户端时间戳
+ (NSString *)clientTime;
//获取UUID
+ (NSString *)pbOldGetUUID;
//获取唯一标识
+ (NSString *)getUniqueHardwareId;
// 获取IDFA
+ (NSString *)getIDFA;
// 获取IDFV
+ (NSString *)getIDFV;
//** 获取设备型号 */ x86_64 iPhone9,1
+ (NSString *)deviceCode;
// 获取user_agent
+(NSString *)getUser_agnet;
// os_type 用户设备类型 1:web 2:wap , 3:android,4:iphone,5:ipad,6:pc,7:wp,0:other
+(NSString *)getOs_Type;
// 获取MAC地址
+ (NSString *)getMACAddress;
//获取android_id
+ (NSString *)getAndroid_ID;
//获取device_uuid
+ (NSString *)getDevice_UUID;
//获取token
+ (NSString *)getToken;
//获取account
+ (NSString *)getAccount;
//设置token
+ (void)setToken:(NSString *)token;
//设置account
+ (void)setAccount:(NSString *)account;
//获取上次登录的channel
+ (NSString *)getChannel;
//设置channel
+ (void)setChannel:(NSString *)channel;
//获取OpenId
+ (NSString *)getOpenID;
//设置openid
+ (void)setOpenID:(NSString *)openid;
//获取上次登录的CitationCode
+ (NSString *)getCitationCode;
//设置CitationCode
+ (void)setCitationCode:(NSString *)citationCode;
//获取上次登录的CitationPassword
+ (NSString *)getCitationPwd;
//设置CitationPassword
+ (void)setCitationPwd:(NSString *)citationPass;

+ (NSString *)att;

+ (NSString *)aie;

//获取设备当前网络IP地址（包括4G 等）
+ (NSString *)getIPAddress:(BOOL)preferIPv4;

+ (BOOL)checkNetwork;

+ (BOOL)isSimulator;

+(NSString*)getLanguage;

+ (BOOL)isJailbroken;

+ (NSString *)getOpenUDID;

+ (NSString *)bootTimeInSec;
+ (NSString *)sysFileTimeMicroseconds;
+ (NSString *)timeZone;
+ (NSString *)deviceMd5Name;
+ (NSString *)systemVersion;
+ (NSString *)diskTotalSize;
+ (NSString *)carrier;
+ (NSString *)countryCode;
+ (NSString *)machine;
+ (NSString *)fileInitTime;
+ (NSString *)model;
+ (NSString *)mntId;

@end
