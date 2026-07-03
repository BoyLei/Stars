//
//  OIConfiguration.h
//  OneId
//
//  Created by YOKA on 2024/4/12.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

typedef NS_ENUM(NSInteger, OIServerRegion) {
    OIServiceRegionInland = 2,
    OIServiceRegionGlobal = 4
};

@interface OIConfiguration : NSObject
@property (nonatomic, copy, nullable) NSString *host; // ⚠️ 务必传入标准 host，不要带入 scheme、path、query 等
@property (nonatomic, copy, nullable) NSString *appId;
@property (nonatomic, assign) OIServerRegion region;
@property (nonatomic, copy) NSString *channel;
@property (nonatomic, copy) NSString *cpsid;
@end

NS_ASSUME_NONNULL_END
