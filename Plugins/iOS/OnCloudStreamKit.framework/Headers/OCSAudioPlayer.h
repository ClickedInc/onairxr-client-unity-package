//  Copyright © 2020 Clicked Inc. All rights reserved.

#import <Foundation/Foundation.h>
#import <CoreAudio/CoreAudioTypes.h>

NS_ASSUME_NONNULL_BEGIN

@interface OCSAudioPlayer : NSObject

- (instancetype)initWithStreamDescription:(AudioStreamBasicDescription *)description;
- (void)play:(NSData *)data;

@end

NS_ASSUME_NONNULL_END
