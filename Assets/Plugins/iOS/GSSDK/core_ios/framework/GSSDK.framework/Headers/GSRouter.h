//
//  GSRouter.h
//
//  Created by yoka on 2023/4/19.
//  Copyright © 2023 yoka. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "GSEnum.h"

@interface GSRouter : NSObject

@property (nonatomic, assign) GSRunMode mode;
@property (nonatomic, copy) NSString *serverUrl;
@property (nonatomic, copy) NSString *orderIDUrl;
@property (nonatomic, copy) NSString *tokenUrl;
@property (nonatomic, copy) NSString *appsflyerInfoUrl;
@property (nonatomic, copy) NSString *configUrl;

+ (instancetype)sharedInstance;
- (instancetype)init NS_UNAVAILABLE;

@end
