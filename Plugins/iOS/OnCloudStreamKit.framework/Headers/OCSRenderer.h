//  Copyright © 2020 Clicked Inc. All rights reserved.

#import <CoreVideo/CoreVideo.h>
#import <Metal/Metal.h>

NS_ASSUME_NONNULL_BEGIN

@interface OCSRenderer : NSObject

- (instancetype)initWithDevice:(id<MTLDevice>)device pixelFormat:(MTLPixelFormat)pixelFormat;
- (void)convertToTexture:(CVImageBufferRef)imageBuffer;
- (void)setRenderAspect:(float)aspect;
- (void)renderToEncoder:(id<MTLRenderCommandEncoder>)renderEncoder flip:(BOOL)flip;

@end

NS_ASSUME_NONNULL_END
