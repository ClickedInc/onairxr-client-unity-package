//  Copyright © 2020 Clicked Inc. All rights reserved.

#import <Foundation/Foundation.h>
#import "types.h"

@interface MulticastPlugin : NSObject

- (instancetype)init;
- (void)enumerateIPv6Interfaces:(uint8_t*)result maxSize:(int)size;
- (int)startup:(const char*)address port:(int)port netaddr:(const char*)netaddr;
- (void)shutdown;
- (BOOL)peekMessage:(struct OCSUnityPluginMessage *)message;
- (void)popMessage;
- (void)join;
- (void)leave;
- (void)setSubgroup:(uint8_t)subgroup;
- (int64_t)beginPendInput;
- (void)pendInput:(uint8_t)device control:(uint8_t)control byteStream:(uint8_t)value;
- (void)pendInput:(uint8_t)device control:(uint8_t)control position:(OCS_VECTOR3D)position rotation:(OCS_VECTOR4D)rotation;
- (void)sendPendingInputs:(int64_t)timestamp;
- (BOOL)getInput:(const char*)member device:(uint8_t)device control:(uint8_t)control byteStream:(uint8_t*)value;
- (BOOL)getInput:(const char*)member device:(uint8_t)device control:(uint8_t)control position:(OCS_VECTOR3D*)position rotation:(OCS_VECTOR4D*)rotation;
- (void)update;

@end
