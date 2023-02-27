#import <UnityAppController.h>
#import "IUnityInterface.h"

extern void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces);
extern void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload(void);

@interface OCSUnityAppController : UnityAppController
@end

@implementation OCSUnityAppController
- (void)shouldAttachRenderDelegate {
    // unlike desktops where plugin dynamic library is automatically loaded and registered
    // we need to do that manually on iOS
    UnityRegisterRenderingPluginV5(&UnityPluginLoad, &UnityPluginUnload);
}
@end
IMPL_APP_CONTROLLER_SUBCLASS(OCSUnityAppController);
