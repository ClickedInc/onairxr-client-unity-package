//  Copyright © 2020 Clicked Inc. All rights reserved.

#import <CoreMedia/CoreMedia.h>

@class OCSVideoDecoder;

NS_ASSUME_NONNULL_BEGIN

@protocol OCSVideoDecoderDelegate <NSObject>

@required
- (void)videoDecoder:(OCSVideoDecoder *)decoder sampleBuffer:(CMSampleBufferRef)sampleBuffer didDecode:(CVImageBufferRef)imageBuffer;
- (void)videoDecoder:(OCSVideoDecoder *)decoder sampleBuffer:(CMSampleBufferRef)sampleBuffer decodeFailedWithError:(NSError *)error;

@end

NS_ASSUME_NONNULL_END
