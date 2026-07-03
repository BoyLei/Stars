//
//  OIResult.h
//  OneId
//
//  Created by YOKA on 2024/4/12.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

@interface OIResult : NSObject
@property (nonatomic, copy, nullable) NSString *sOneId; // generate by server
@property (nonatomic, copy, nullable) NSString *cOneId; // generate by client
@end

NS_ASSUME_NONNULL_END
