//  Copyright © 2020 Clicked Inc. All rights reserved.

#include "OCSVideoDecoderDelegate.h"

NS_ASSUME_NONNULL_BEGIN

@interface OCSVideoDecoder : NSObject

@property (nonatomic, weak, nullable) id<OCSVideoDecoderDelegate> delegate;

- (instancetype)initWithVideoFormatDescription:(CMVideoFormatDescriptionRef)videoFormatDescription;
- (instancetype)initWithVideoFormatDescription:(CMVideoFormatDescriptionRef)videoFormatDescription outYUV:(BOOL)yuv;

- (void)decode:(CMSampleBufferRef)sampleBuffer;

@end

NS_ASSUME_NONNULL_END
