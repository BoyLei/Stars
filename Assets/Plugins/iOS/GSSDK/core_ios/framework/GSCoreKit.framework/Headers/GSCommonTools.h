//
//  GSCommonTools.h
//  GS
//
//  Created by VisonCui on 2024/4/29.
//

#ifndef GSCommonTools_h
#define GSCommonTools_h

// json对象转json字符串
static inline NSString *GSJsonObjToString(id jsonObj) {
    
    if (!jsonObj ||
        (![jsonObj isKindOfClass:[NSDictionary class]] &&
        ![jsonObj isKindOfClass:[NSArray class]])) {
        return nil;
    }
    
    if (![NSJSONSerialization isValidJSONObject:jsonObj]) {
        NSLog(@"The given object can not be converted to JSON data!");
        return nil;
    }

    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:jsonObj options:kNilOptions error:nil];
    
    if (!jsonData) return nil;
    
    return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    
}

// json字符串转json对象
static inline id GSJsonStringToObj(NSString *jsonString) {
    if (!jsonString || ![jsonString isKindOfClass:[NSString class]]) {
        return nil;
    }
    
    NSData *jsonData = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
    if (!jsonData) {
        return nil;
    }
    
    return [NSJSONSerialization JSONObjectWithData:jsonData options:NSJSONReadingMutableLeaves error:nil];
}

#endif /* GSCommonTools_h */
