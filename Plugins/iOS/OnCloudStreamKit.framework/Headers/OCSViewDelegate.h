//  Copyright © 2020 Clicked Inc. All rights reserved.

#import <Foundation/Foundation.h>

@class OCSView;

@protocol OCSViewDelegate <NSObject>

- (void)ocsView:(OCSView *)view connectionStateChanged:(BOOL)connected;
- (void)ocsView:(OCSView *)view playStateChanged:(BOOL)playing;

@end
