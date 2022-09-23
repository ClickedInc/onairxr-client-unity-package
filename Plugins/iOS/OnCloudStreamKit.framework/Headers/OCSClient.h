//  Copyright © 2020 Clicked Inc. All rights reserved.

#import <Foundation/Foundation.h>
#import <CoreGraphics/CoreGraphics.h>
#import <CoreMedia/CoreMedia.h>

@class OCSVideoDecoder;
@class OCSAudioPlayer;
@protocol OCSClientDelegate;

@interface OCSClient : NSObject

@property (nullable, weak, nonatomic, readonly) OCSVideoDecoder *videoDecoder;
@property (nullable, weak, nonatomic, readonly) OCSAudioPlayer *audioPlayer;
@property (nullable, weak, nonatomic) id<OCSClientDelegate> delegate;

NS_ASSUME_NONNULL_BEGIN

- (instancetype)initWithLicenseFile:(NSString *)path;
- (void)connectTo:(NSString *)hostname port:(NSInteger)port config:(NSDictionary *)config;
- (void)connectTo:(NSString *)hostname port:(NSInteger)port json:(NSString *)json;
- (void)disconnect;
- (void)play;
- (void)stop;
- (void)releaseVideoSampleBuffer:(CMSampleBufferRef)sampleBuffer;
- (void *)session;
- (void)notify:(void *)notification;
- (void)sendInput:(void *)buffer timestamp:(int64_t)timestamp reliable:(BOOL)reliable;

NS_ASSUME_NONNULL_END

@end
