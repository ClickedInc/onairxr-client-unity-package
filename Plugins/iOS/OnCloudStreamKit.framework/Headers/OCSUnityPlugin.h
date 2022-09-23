//  Copyright © 2020 Clicked Inc. All rights reserved.

#import <Foundation/Foundation.h>
#import "types.h"

struct IUnityInterfaces;

@interface OCSUnityPlugin : NSObject

@property (atomic) BOOL connected;
@property (atomic) BOOL playing;
@property (copy) NSString *profile;

- (instancetype)initWithUnityInterfaces:(struct IUnityInterfaces *)unityInterfaces;
- (void)connectTo:(NSString *)address port:(NSInteger)port;
- (void)disconnect;
- (void)play;
- (void)stop;
- (void)renderWithEventID:(int)eventID;
- (BOOL)peekMessage:(struct OCSUnityPluginMessage *)message;
- (void)popMessage;
- (void)beginPendInput:(int64_t *)timestamp;
- (void)pendInput:(uint8_t)device control:(uint8_t)control state:(uint8_t)state;
- (void)pendInput:(uint8_t)device control:(uint8_t)control byteAxis:(uint8_t)axis;
- (void)pendInput:(uint8_t)device control:(uint8_t)control axis:(float)axis;
- (void)pendInput:(uint8_t)device control:(uint8_t)control axis2D:(OCS_VECTOR2D)axis2D;
- (void)pendInput:(uint8_t)device control:(uint8_t)control position:(OCS_VECTOR3D)position rotation:(OCS_VECTOR4D)rotation;
- (void)pendInput:(uint8_t)device control:(uint8_t)control position:(OCS_VECTOR2D)position state:(uint8_t)state active:(bool)active;
- (void)sendPendingInputs:(int64_t)timestamp;
- (void)clearInput;
- (void)updateInputFrame;
- (void)setCameraProjectionWithLeft:(float)left top:(float)top right:(float)right bottom:(float)bottom;
- (void)setRenderAspect:(float)aspect;

+ (BOOL)HEVCDecodeSupported;

@end
