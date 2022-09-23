//  Copyright © 2020 Clicked Inc. All rights reserved.

#import <Foundation/NSObject.h>
#import <CoreMedia/CoreMedia.h>

NS_ASSUME_NONNULL_BEGIN

@protocol OCSClientDelegate <NSObject>

@optional
- (void)ocsClient:(OCSClient *)client connectedWithVideoFormatDescription:(nullable CMVideoFormatDescriptionRef)videoDescription videoFlipped:(BOOL)videoFlipped audioStreamDescription:(nullable AudioStreamBasicDescription *)audioDescription;
- (void)disconnectedOCSClient:(OCSClient *)client;
- (void)ocsClient:(OCSClient *)client playStateChanged:(BOOL)playing;
- (void)ocsClient:(OCSClient *)client videoReceived:(CMSampleBufferRef)sampleBuffer;
- (void)ocsClient:(OCSClient *)client audioReceived:(NSData *)data;

@end

NS_ASSUME_NONNULL_END
