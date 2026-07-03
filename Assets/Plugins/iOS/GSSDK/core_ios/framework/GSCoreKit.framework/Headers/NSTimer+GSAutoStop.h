//
//  NSTimer+AutoStop.h
//  ECalendar-Pro
//
//  Created by B.E.N on 15/5/7.
//  Copyright (c) 2015年 etouch. All rights reserved.
//

#import <Foundation/Foundation.h>

typedef void(^GSTimeBlock)(NSTimer* timer);

@class GSAutoStopTarget;

@interface GSAutoStopTracker : NSObject

@property (nonatomic, weak) GSAutoStopTarget *autoStopTarget;

@end

@interface GSAutoStopTarget : NSObject

@property (nonatomic, weak) NSTimer *timer;
@property (nonatomic, weak) id target;
@property (nonatomic) SEL selector;
@property (nonatomic, copy) GSTimeBlock  block;

@end


@interface NSTimer (GSAutoStop)

+ (NSTimer *)scheduledTimerWithAutoTimeInterval:(NSTimeInterval)timeInterval
                                        target:(id)target
                                      selector:(SEL)selector
                                      userInfo:(id)userInfo
                                       repeats:(BOOL)repeats;


+ (void)scheduledTimerWithAutoTimeInterval:(NSTimeInterval)timeInterval
                                        target:(id)target
                                         block:(GSTimeBlock)block
                                      userInfo:(id)userInfo
                                       repeats:(BOOL)repeats;
@end


