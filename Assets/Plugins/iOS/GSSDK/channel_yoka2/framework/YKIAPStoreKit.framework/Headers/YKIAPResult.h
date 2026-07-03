//
//  YKIAPResult.h
//  YKIAPStoreKit
//
//  Created by qingyun on 2024/6/20.
//

#import <Foundation/Foundation.h>
@class YKTransaction;
NS_ASSUME_NONNULL_BEGIN
typedef NS_ENUM(NSUInteger, YKIAPStoreKitErrorCode) {
    YKIAP_StoreKitSuccess            = 0,          /**< 成功   */
    YKIAP_StoreKitParamErr           = 1,          /**< productid不能为空   */
    YKIAP_StoreKitCode1              = 2174601,    /**< 商品信息获取失败    */
    YKIAP_StoreKitCode2              = 2174602,    /**< 取消购买   */
    YKIAP_StoreKitCode3              = 2174603,    /**< 购买失败   */
    YKIAP_StoreKitCode4              = 2174604,    /**< UUID格式错误   */
    YKIAP_StoreKitCode5              = 2174605,    /**< 支付异常   */
    YKIAP_StoreKitCode6              = 2174606,    /**< 请先获取商品信息   */
};

typedef NS_ENUM(NSUInteger, YKIAPPurchaseType) {
    YKIAP_PurchaseStoreKit1          = 0,           /**< Storekit1   */
    YKIAP_PurchaseStoreKit2          = 1,           /**< Storekit2    */
};



@interface YKIAPResult : NSObject
@property (nonatomic, assign) NSInteger errorCode;
@property (nonatomic, strong) NSString *errorMsg;
@property (nonatomic, assign) YKIAPPurchaseType type;
@property (nonatomic, strong) YKTransaction *transaction;

@end

NS_ASSUME_NONNULL_END
