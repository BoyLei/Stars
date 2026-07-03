//
//  ImagePathUtil.h
//
//  Created by zls on 19/4/14.
//  Copyright © 2019年 zls. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface GSImagePathUtil : NSObject

+ (NSString *)bundleImageNamed:(NSString *)name;

+ (NSString *)bundleImageNamed:(NSString *)name bundleNamed:(NSString *)bundleName;

+ (NSString *)bundleImageNamed:(NSString *)name bundleNamed:(NSString *)bundleName bundleForClass:(Class)cla;

+ (NSString *)bundleResourceNamed:(NSString *)name bundleNamed:(NSString *)bundleName bundleForClass:(Class)cla resourceType:(NSString *)type;

@end
