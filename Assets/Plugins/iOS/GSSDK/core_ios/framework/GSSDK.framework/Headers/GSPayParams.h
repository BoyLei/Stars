//
//  PayParams.h
//  GSSDK
//
//  Created by sen on 2021/12/14.
//

#import <Foundation/Foundation.h>

@interface GSPayParams : NSObject

/// SDK订单号，请勿赋值
@property (nonatomic, copy) NSString *orderID;
/// SDK userId，请勿赋值
@property (nonatomic, copy, readonly) NSString *userId;

/// 共有参数
@property (nonatomic, strong) NSNumber *price;
@property (nonatomic, copy) NSString *productId;
@property (nonatomic, copy) NSString *productName;
@property (nonatomic, copy) NSString *productDesc;

@property (nonatomic, copy) NSString *serverId;
@property (nonatomic, copy) NSString *serverName;
@property (nonatomic, copy) NSString *roleId;
@property (nonatomic, copy) NSString *roleName;
@property (nonatomic, copy) NSString *roleLevel;
@property (nonatomic, copy) NSString *payNotifyUrl;
@property (nonatomic, copy) NSString *extension;

////新增参数  ---新增为IAP查询使用
@property (nonatomic, copy) NSString *currency;
//@property (nonatomic, copy) NSString *details;
//@property (nonatomic, assign) BOOL isAvailable;  //是否查询成功
//@property (nonatomic, copy) NSString *detailsDescription; //商品价格符号
//@property (nonatomic, copy) NSString *currencyCode;  //商品价格代码
//
/// PB渠道参数
@property (nonatomic, assign) NSInteger coinNum;
@property (nonatomic, copy) NSString *coinName;
//
///// 君子互娱渠道参数
//@property (nonatomic, assign) NSInteger buyNum;
//
////新增参数 -----新增payType ygame 魔方需要此参数
//@property (nonatomic, copy) NSString *payType;  //pay类型
//@property (nonatomic, copy) NSString *gameNotifyUrl;
//
////可选属性
//@property (nonatomic, copy)NSString *vip;
//@property (nonatomic, copy)NSString *orderSign;
//@property (nonatomic, copy)NSString *orderName;

@property (nonatomic, copy) NSDictionary *payAllDic;

- (instancetype)initWithDic:(NSDictionary *)dic;

@end


@interface GSPayAnalyticsData : NSObject


+ (instancetype)sharedInstance;

- (NSDictionary *)analyticsDataDicWithOther:(NSDictionary *)dic;

- (NSDictionary *)analyticsDataDicWithParam:(GSPayParams *)param;

@end
