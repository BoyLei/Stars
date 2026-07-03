//
//  YKTransaction.h
//  YKIAPStoreKit
//
//  Created by qingyun on 2024/6/20.
//

#import <Foundation/Foundation.h>
#import <StoreKit/StoreKit.h>
NS_ASSUME_NONNULL_BEGIN

@interface YKTransaction : NSObject
@property (nonatomic, strong) NSString *productId;             //商品名：com.xxx.xxx
@property (nonatomic, strong) NSString *transactionId;         //苹果返回的ID：2000000637533031
@property (nonatomic, strong) NSString *receipt;               //base64 
@property (nonatomic, strong) NSString *applicationUsername;   //如：com.xxx.xxx
@property (nonatomic, strong) NSString *localeCurrencyCode;    //货币类型：CHY
@property (nonatomic, strong) NSString *price;                 //价格


- (instancetype)initWithSKPaymentTransaction:(SKPaymentTransaction *)transaction;
@end

NS_ASSUME_NONNULL_END
