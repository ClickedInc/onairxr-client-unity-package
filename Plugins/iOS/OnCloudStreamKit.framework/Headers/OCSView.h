//  Copyright © 2020 Clicked Inc. All rights reserved.

#import "OCSViewDelegate.h"

@import MetalKit;

NS_ASSUME_NONNULL_BEGIN

//IB_DESIGNABLE
@interface OCSView : MTKView

@property (nullable, weak, nonatomic) id<OCSViewDelegate> ocsViewDelegate;

- (void)connectTo:(NSString *)address port:(NSInteger)port size:(CGSize)size frameRate:(float)frameRate;
- (void)disconnect;

@end

NS_ASSUME_NONNULL_END
