#import <UIKit/UIKit.h>
#import <OnCloudStreamKit/OCSUnityPlugin.h>
#import <OnCloudStreamKit/MulticastPlugin.h>
#import <UnityAppController.h>
#import "IUnityInterface.h"
#import "IUnityGraphics.h"

static OCSUnityPlugin *s_Plugin;
static MulticastPlugin *s_MulticastPlugin;

static void UNITY_INTERFACE_API PrepareRender(int eventID)       {}
static void UNITY_INTERFACE_API PreRenderVideoFrame(int eventID) {}
static void UNITY_INTERFACE_API EndOfRenderFrame(int eventID)    {}

static void UNITY_INTERFACE_API RenderVideoFrame(int eventID) {
    [s_Plugin renderWithEventID:eventID];
}

static void UNITY_INTERFACE_API SetRenderAspect(int eventID) {
    float aspect = eventID / 1000000.0f;
    [s_Plugin setRenderAspect:aspect];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces) {
    s_Plugin = [[OCSUnityPlugin alloc] initWithUnityInterfaces:unityInterfaces];
    s_MulticastPlugin = [[MulticastPlugin alloc] init];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload() {
    s_Plugin = nil;
    s_MulticastPlugin = nil;
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_DeviceRefreshRate() {
    return (int)UIScreen.mainScreen.maximumFramesPerSecond;
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_HEVCDecodeSupported() {
    return [OCSUnityPlugin HEVCDecodeSupported];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_SetProfile(const char* profile) {
    s_Plugin.profile = [NSString stringWithUTF8String:profile];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_InitPlatform() {
    // do nothing
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_Init(const char* licenseFilePath, int audioOutputSampleRate, bool hasInput) {
    return 1;
}

// session control
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RequestConnect(const char* address, int port) {
    [s_Plugin connectTo:[NSString stringWithUTF8String:address] port:port];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RequestPlay() {
    [s_Plugin play];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RequestStop() {
    [s_Plugin stop];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RequestDisconnect() {
    [s_Plugin disconnect];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_Cleanup() {
    // do nothing
}

// for audio
bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_GetAudioData(void* buffer, int length, int channels) {
    // TODO
    return false;
}

// input
bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_GetInputState(unsigned char device, unsigned char control, unsigned char* state) {
    // TODO
    return false;
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_GetInputRaycastHit(unsigned char device, unsigned char control, OCS_VECTOR3D* origin, OCS_VECTOR3D* hitPosition, OCS_VECTOR3D* hitNormal) {
    // TODO
    return false;
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_GetInputVibration(unsigned char device, unsigned char control, float* frequency, float* amplitude) {
    // TODO
    return false;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_BeginPendInput(long long* timestamp) {
    [s_Plugin beginPendInput:timestamp];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_PendInputState(unsigned char device, unsigned char control, unsigned char state) {
    [s_Plugin pendInput:device control:control state:state];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_PendInputByteAxis(unsigned char device, unsigned char control, unsigned char axis) {
    [s_Plugin pendInput:device control:control byteAxis:axis];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_PendInputAxis(unsigned char device, unsigned char control, float axis) {
    [s_Plugin pendInput:device control:control axis:axis];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_PendInputAxis2D(unsigned char device, unsigned char control, OCS_VECTOR2D axis2D) {
    [s_Plugin pendInput:device control:control axis2D:axis2D];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_PendInputPose(unsigned char device, unsigned char control, OCS_VECTOR3D position, OCS_VECTOR4D rotation) {
    [s_Plugin pendInput:device control:control position:position rotation:rotation];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_PendInputTouch2D(unsigned char device, unsigned char control, OCS_VECTOR2D position, unsigned char state, bool active) {
    [s_Plugin pendInput:device control:control position:position state:state active:active];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_SendPendingInputs(long long timestamp) {
    [s_Plugin sendPendingInputs:timestamp];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_ClearInput() {
    [s_Plugin clearInput];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_UpdateInputFrame() {
    [s_Plugin updateInputFrame];
}

// dispatch messages
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_SendUserData(void* data, int length) {
    // TODO
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_CheckMessageQueue(void** source, const void** data, int* length) {
    struct OCSUnityPluginMessage msg;
    
    if ([s_Plugin peekMessage:&msg]) {
        *source = msg.source;
        *data = msg.data;
        *length = msg.length;
        return true;
    }
    return false;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RemoveFirstMessageFromQueue() {
    [s_Plugin popMessage];
}

// query/set plugin properties
bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_IsConnected() {
    return [s_Plugin connected] == YES;
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_IsPlaying() {
    return [s_Plugin playing] == YES;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_EnableNetworkTimeWarp(bool enable) {
    // do nothing
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_EnableCopyrightCheck(bool enable) {
    // do nothing
}

// video stream rendering
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_SetCameraOrientation(float x, float y, float z, float w, int* viewNumber) {
    // TODO
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_SetCameraProjection(float left, float top, float right, float bottom) {
    [s_Plugin setCameraProjectionWithLeft:left top:top right:right bottom:bottom];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_GetVideoRenderTargetTexture(void** texture, int* width, int* height) {
    // do nothing
}

// render events
UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_SetRenderAspect_RenderThread_Func() {
    return SetRenderAspect;
}

UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_PrepareRender_RenderThread_Func() {
    return PrepareRender;
}

UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_PreRenderVideoFrame_RenderThread_Func() {
    return PreRenderVideoFrame;
}

UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_RenderVideoFrame_RenderThread_Func() {
    return RenderVideoFrame;
}

UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_EndOfRenderFrame_RenderThread_Func() {
    return EndOfRenderFrame;
}

// for multicast
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastEnumerateIPv6Interfaces(StringBuffer* result) {
    [s_MulticastPlugin enumerateIPv6Interfaces:result->buffer maxSize:result->size];
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastStartup(const char* address, int port, const char* netaddr) {
    return [s_MulticastPlugin startup:address port:port netaddr:netaddr];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastShutdown() {
    [s_MulticastPlugin shutdown];
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastCheckMessageQueue(unsigned long long** source, const void** data, int* length) {
    struct OCSUnityPluginMessage msg;
    
    if ([s_MulticastPlugin peekMessage:&msg]) {
        *source = msg.source;
        *data = msg.data;
        *length = msg.length;
        return true;
    }
    return false;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastRemoveFirstMessageFromQueue() {
    [s_MulticastPlugin popMessage];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastJoin() {
    [s_MulticastPlugin join];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastLeave() {
    [s_MulticastPlugin leave];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastSetSubgroup(unsigned char subgroup) {
    [s_MulticastPlugin setSubgroup:subgroup];
}

long long UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastBeginPendInput() {
    return [s_MulticastPlugin beginPendInput];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastPendInputByteStream(unsigned char device, unsigned char control, unsigned char value) {
    [s_MulticastPlugin pendInput:device control:control byteStream:value];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastPendInputPose(unsigned char device, unsigned char control, OCS_VECTOR3D position, OCS_VECTOR4D rotation) {
    [s_MulticastPlugin pendInput:device control:control position:position rotation:rotation];
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastSendPendingInputs(long long timestamp) {
    [s_MulticastPlugin sendPendingInputs:timestamp];
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastGetInputByteStream(const char* member, unsigned char device, unsigned char control, unsigned char* value) {
    return [s_MulticastPlugin getInput:member device:device control:control byteStream:value] == YES;
}

bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastGetInputPose(const char* member, unsigned char device, unsigned char control, OCS_VECTOR3D* position, OCS_VECTOR4D* rotation) {
    return [s_MulticastPlugin getInput:member device:device control:control position:position rotation:rotation] == YES;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ocs_MulticastUpdate() {
    [s_MulticastPlugin update];
}

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
