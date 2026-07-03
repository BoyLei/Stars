//
//  GSLanguage.h
//
//
//  Created by VisonCui on 202g/2/28.
//  Copyright © 2022 FunPlus. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface GSLanguage : NSObject

/// 手机系统语言 形式如（"zh"，中文繁体:"zh-tw"，英语:"en",法语:"fr"，德语:"de"）
+ (NSString *)getSysLanguage;

/// 本地设置语言（逻辑：优先本地缓存语言，如没有，则返回系统语言简码）
+ (NSString *)getLocalLanguage;

// 是否为阿拉伯语言
+ (BOOL)isArLanguage;

/// 设置语言  (此方法谨慎使用，通常仅供FPX全局方法使用)
/// @param lang  语言简码
///中文："zh"，中文繁体:"zh-tw"，英语:"en",法语:"fr"，德语:"de"，西班牙语:"es"，波兰语:"pl"，
///葡萄牙语:"pt"，土耳其语:"tr"，俄语:"ru"，阿拉伯语:"ar"，韩语:"ko"，泰语:"th"，印尼:"id"，越南:"vi"，
///日语:"ja"，马来:"my"，意大利:"it"，荷兰语:"nl"，瑞典语:"sv"
+ (void)setGLanguage:(NSString *)lang;

@end
