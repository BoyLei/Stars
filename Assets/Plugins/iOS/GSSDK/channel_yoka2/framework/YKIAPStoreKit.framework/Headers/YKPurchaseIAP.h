//
//  YKPurchaseIAP.h
//  YKIAPStoreKit
//
//  Created by qingyun on 2024/6/20.
//

#import <Foundation/Foundation.h>
@class YKTransaction;
@class YKIAPResult;

@protocol YKPurchaseIAPDelegate;

typedef NS_ENUM(NSUInteger, YKIAPStoreKitType) {
    YKIAP_StoreKitDefault            = 0,    /**< 默认iOS15以上用StoreKit2   */
    YKIAP_StoreKit1                  = 1,    /**< StoreKit1    */
};

NS_ASSUME_NONNULL_BEGIN

typedef void(^YKPurchaseIAPBlock)(YKIAPResult *result);

@interface YKPurchaseIAP : NSObject

+ (instancetype)sharedInstance;
- (instancetype)init NS_UNAVAILABLE;

/**
 * 获取商品信息
 * param productId  苹果后台配置的productId
 * param type  请求方式
 * completion  回调block
 */
- (void)requestProduct:(NSString *)productId withType:(YKIAPStoreKitType)type completion:(YKPurchaseIAPBlock)completion;

/**
 * 发起支付请求
 * param productId  苹果后台配置的productId
 * param type  请求方式
 * aram applicationUsername  自定义order
 * callback  purchaseSuccess purchasefailed
 */
- (void)requestPurchase:(NSString *)productId withType:(YKIAPStoreKitType)type withUsername:(NSString*)applicationUsername  withUDID:(NSString*)udid withDelegate:(id)delegate;

/**
 * 发起支付请求
 * param productId  苹果后台配置的productId
 * param type  请求方式
 * aram applicationUsername  自定义order
 * callback  purchaseSuccess purchasefailed
 */
-(void)listenTransation:(id)delegate;

/**
 * 结束订单
 * 注意！ 一定要在支付验证成功后及时结束当前订单
 */
- (void)finishTransaction;

@end

@protocol YKPurchaseIAPDelegate <NSObject>

@optional
- (void)purchaseSuccess:(YKIAPResult *)result;
- (void)purchasefailed:(YKIAPResult *)result;
@end

NS_ASSUME_NONNULL_END
