//
//  UIView+GSWebImageCache.h
//  GSCoreKit
//
//  Created by yoka on 2023/2/10.
//

#import <UIKit/UIKit.h>

typedef void(^GSWebImageCacheBlock)(UIImage *image, BOOL isCache, NSError *error);

@interface UIView (GSMWebImageCache)

- (void)setImageWithUrl:(NSURL *)url;
- (void)setImageWithUrl:(NSURL *)url placeholder:(UIImage *)holderImage;
- (void)setImageWithUrl:(NSURL *)url placeholder:(UIImage *)holderImage completion:(GSWebImageCacheBlock)block;
- (void)setImageWithURL:(NSURL *)url placeholderImage:(UIImage *)holderImage;
- (void)setImageWithURL:(NSURL *)url placeholderImage:(UIImage *)holderImage completion:(GSWebImageCacheBlock)block;

@end

@interface CachedImageManager : NSObject

@property (nonatomic, copy, readonly) NSString *cachePath;//缓存目录

+ (CachedImageManager *)shareInstance;
- (BOOL)cacheUrl:(NSURL *)url withData:(NSData *)data;    //存入URL
- (NSString *)imagePathForUrl:(NSURL *)url;               //取出URL对应的path
- (void)clearCache;                                       //清除缓存

@end
