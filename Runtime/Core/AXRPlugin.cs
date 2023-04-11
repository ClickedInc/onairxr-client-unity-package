/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace onAirXR.Client {
    public class AXRClientPlugin {
        private const uint RenderEventMaskClearColor = 0x00800000U;
        private const uint RenderEventMaskRenderOnTexture = 0x00400000U;
        private const int RenderEventRenderAspectScale = 1000000;
        private const int RenderEventOpacityScale = 100;

        public enum FrameType {
            StereoLeft = 0,
            StereoRight,
            Mono
        }

        public enum InputSendPolicy {
            Never = 0,
            Always,
            NonzeroAlwaysZeroOnce,
            OnChange
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        private const string Name = "axr";
#elif UNITY_ANDROID
        private const string Name = "axr";

        [DllImport(Name)] private static extern void axr_InitJNI();   // TODO merge name as axr_InitPlatform()
#elif UNITY_IOS
        private const string Name = "__Internal";

        [DllImport(Name)] private static extern int axr_DeviceRefreshRate();
        [DllImport(Name)] private static extern bool axr_HEVCDecodeSupported();
#else
        private const string Name = "NotImplementedYet";
#endif

        private static float optimisticVideoFrameRate {
            get {
#if UNITY_EDITOR || UNITY_STANDALONE
                return 60.0f;
#elif UNITY_ANDROID
                AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject display = activity.Call<AndroidJavaObject>("getSystemService", "window").Call<AndroidJavaObject>("getDefaultDisplay");
                return display.Call<float>("getRefreshRate");
#elif UNITY_IOS
                return axr_DeviceRefreshRate();
#else
                return 0.0f;
#endif
            }
        }

        private static string[] supportedVideoCodecs {
            get {
                var supportAVC = false;
                var supportHEVC = false;
#if UNITY_EDITOR || UNITY_STANDALONE
                supportAVC = true; // Currently Windows supports AVC only
#elif UNITY_ANDROID
                var mediaCodecList = new AndroidJavaObject("android.media.MediaCodecList", 0);
                var mediaCodecInfos = mediaCodecList.Call<AndroidJavaObject[]>("getCodecInfos");
                foreach (var codecInfo in mediaCodecInfos) {
                    var types = codecInfo.Call<string[]>("getSupportedTypes");
                    foreach (var type in types) {
                        if (type.Equals("video/avc")) {
                            supportAVC = true;
                        }
                        else if (type.Equals("video/hevc")) {
                            supportHEVC = true;
                        }
                    }
                }
#elif UNITY_IOS
                supportAVC = true;
                supportHEVC = axr_HEVCDecodeSupported();
#endif

                Assert.IsTrue(supportHEVC || supportAVC);

                var result = new string[(supportHEVC && supportAVC) ? 2 : 1];
                if (supportHEVC) {
                    result[0] = "H265";
                }
                if (supportAVC) {
                    result[result.Length - 1] = "H264";
                }
                return result;
            }
        }

        public static float GetOptimisticVideoFrameRate() {
            return optimisticVideoFrameRate;
        }

        public static string[] GetSupportedVideoCodecs() {
            return supportedVideoCodecs;
        }

        public static string[] GetSupportedAudioCodecs() {
            return new string[] { "opus" };
        }

#if !UNITY_EDITOR_OSX
        [DllImport(Name, EntryPoint = "axr_InitPlatform")] 
        public static extern void Load();

        [DllImport(Name, EntryPoint = "axr_Init")] 
        public static extern int Configure(int audioOutputSampleRate, bool hasInput, bool openglRenderTextureCoord);

        [DllImport(Name, EntryPoint = "axr_Cleanup")] 
        public static extern void Cleanup();

        [DllImport(Name, EntryPoint = "axr_GetOffscreenFramebufferTexture")]
        public static extern uint GetOffscreenFramebufferTexture();

        [DllImport(Name, EntryPoint = "axr_EnableCopyrightCheck")] 
        public static extern void EnableCopyrightCheck(bool enable);

        [DllImport(Name, EntryPoint = "axr_SetProfile")] 
        public static extern void SetProfile(string profile);

        [DllImport(Name, EntryPoint = "axr_IsConnected")] 
        public static extern bool IsConnected();

        [DllImport(Name, EntryPoint = "axr_IsPlaying")] 
        public static extern bool IsPlaying();

        [DllImport(Name, EntryPoint = "axr_RequestConnect")] 
        public static extern void RequestConnect(string address, int port);

        [DllImport(Name, EntryPoint = "axr_RequestDisconnect")] 
        public static extern void RequestDisconnect();

        [DllImport(Name, EntryPoint = "axr_RequestPlay")] 
        public static extern void RequestPlay();

        [DllImport(Name, EntryPoint = "axr_RequestStop")] 
        public static extern void RequestStop();

        [DllImport(Name, EntryPoint = "axr_CheckMessageQueue")] 
        public static extern bool CheckMessageQueue(out IntPtr source, out IntPtr data, out int length);

        [DllImport(Name, EntryPoint = "axr_RemoveFirstMessageFromQueue")] 
        public static extern void RemoveFirstMessageFromQueue();

        [DllImport(Name, EntryPoint = "axr_EnableProfiler")] 
        public static extern void EnableProfiler(bool enable, string dataFilename);

        [DllImport(Name, EntryPoint = "axr_EnableNetworkTimeWarp")] 
        public static extern void EnableNetworkTimeWarp(bool enable);

        [DllImport(Name, EntryPoint = "axr_GetVideoRenderTargetTexture")] 
        public static extern bool GetVideoRenderTargetTexture(ref IntPtr texture, ref int width, ref int height);

        [DllImport(Name, EntryPoint = "axr_GetAudioData")] 
        public static extern bool GetAudioData([MarshalAs(UnmanagedType.LPArray)] float[] buffer, int length, int channels);

        [DllImport(Name, EntryPoint = "axr_BeginPendInput")] 
        public static extern void BeginPendInput(ref long timestamp);

        [DllImport(Name, EntryPoint = "axr_PendInputState")] 
        public static extern void PendInputState(byte device, byte control, byte state);

        [DllImport(Name, EntryPoint = "axr_PendInputByteAxis")] 
        public static extern void PendInputByteAxis(byte device, byte control, byte axis);

        [DllImport(Name, EntryPoint = "axr_PendInputAxis")] 
        public static extern void PendInputAxis(byte device, byte control, float axis);

        [DllImport(Name, EntryPoint = "axr_SendPendingInputs")] 
        public static extern void SendPendingInputs(long timestamp);

        [DllImport(Name, EntryPoint = "axr_GetInputState")] 
        public static extern bool GetInputState(byte device, byte control, ref byte state);

        [DllImport(Name, EntryPoint = "axr_GetInputVibration")] 
        public static extern bool GetInputVibration(byte device, byte control, ref float frequency, ref float amplitude);

        [DllImport(Name, EntryPoint = "axr_UpdateInputFrame")] 
        public static extern void UpdateInputFrame();

        [DllImport(Name, EntryPoint = "axr_ClearInput")] 
        public static extern void ClearInput();

        public static void RequestSendUserData(byte[] data) {
            if (data == null || data.Length <= 0) { return; }

            var ptr = Marshal.AllocHGlobal(data.Length);
            try {
                Marshal.Copy(data, 0, ptr, data.Length);
                axr_SendUserData(ptr, data.Length);
            }
            finally {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static void SetCameraPose(Pose pose, ref int viewNumber) {
            axr_SetCameraPose(new AXRVector3D(pose.position), new AXRVector4D(pose.rotation), ref viewNumber);
        }

        public static void SetCameraProjection(float[] projection) {
            axr_SetCameraProjection(projection[0], projection[1], projection[2], projection[3]);
        }

        public static void SetRenderAspect(float aspect) {
            GL.IssuePluginEvent(axr_SetRenderAspect_RenderThread_Func(), (int)(aspect * RenderEventRenderAspectScale));
        }

        public static void SetOpacity(float opacity) {
            GL.IssuePluginEvent(axr_SetOpacity_RenderThread_Func(), (int)(opacity * RenderEventOpacityScale));
        }

        public static void PrepareRender(bool offscreenRendering) {
            GL.IssuePluginEvent(axr_PrepareRender_RenderThread_Func(), offscreenRendering ? 1 : 0);
        }

        public static void PreRenderVideoFrame(int viewNumber) {
            GL.IssuePluginEvent(axr_PreRenderVideoFrame_RenderThread_Func(), viewNumber);
        }

        public static void RenderVideoFrame(AXRRenderCommand renderCommand, FrameType frameType, bool clearColor = true, bool renderOnTexture = false) {
            renderCommand.Issue(axr_RenderVideoFrame_RenderThread_Func(), renderEvent(frameType, clearColor, renderOnTexture));
        }

        public static void RenderVolume(AXRRenderCommand renderCommand, FrameType frameType, IntPtr data) {
            renderCommand.Issue(axr_RenderVolume_RenderThread_Func(), renderEvent(frameType, false, false), data);
        }

        public static void EndRenderVideoFrame() {
            GL.IssuePluginEvent(axr_EndOfRenderFrame_RenderThread_Func(), 0);
        }

        public static void PendInputAxis2D(byte device, byte control, Vector2 axis2D) {
            axr_PendInputAxis2D(device, control, new AXRVector2D(axis2D));
        }

        public static void PendInputPose(byte device, byte control, Vector3 position, Quaternion rotation) {
            axr_PendInputPose(device, control, new AXRVector3D(position), new AXRVector4D(rotation));
        }

        public static void PendInputTouch2D(byte device, byte control, Vector2 position, byte state, bool active) {
            axr_PendInputTouch2D(device, control, new AXRVector2D(position), state, active);
        }

        public static bool GetInputRaycastHit(byte device, byte control, ref Vector3 origin, ref Vector3 hitPosition, ref Vector3 hitNormal) {
            var ori = new AXRVector3D();
            var pos = new AXRVector3D();
            var norm = new AXRVector3D();

            if (axr_GetInputRaycastHit(device, control, ref ori, ref pos, ref norm) == false) { return false; }

            origin = ori.toVector3();
            hitPosition = pos.toVector3();
            hitNormal = norm.toVector3();
            return true;
        }

        [DllImport(Name)] private static extern void axr_SendUserData(IntPtr data, int length);
        [DllImport(Name)] private static extern void axr_SetCameraPose(AXRVector3D position, AXRVector4D rotation, ref int viewNumber);
        [DllImport(Name)] private static extern void axr_SetCameraProjection(float left, float top, float right, float bottom);
        [DllImport(Name)] private static extern IntPtr axr_SetRenderAspect_RenderThread_Func();
        [DllImport(Name)] private static extern IntPtr axr_SetOpacity_RenderThread_Func();
        [DllImport(Name)] private static extern IntPtr axr_PrepareRender_RenderThread_Func();
        [DllImport(Name)] private static extern IntPtr axr_PreRenderVideoFrame_RenderThread_Func();
        [DllImport(Name)] private static extern IntPtr axr_RenderVideoFrame_RenderThread_Func();
        [DllImport(Name)] private static extern IntPtr axr_EndOfRenderFrame_RenderThread_Func();
        [DllImport(Name)] private static extern IntPtr axr_RenderVolume_RenderThread_Func();
        [DllImport(Name)] private static extern void axr_PendInputAxis2D(byte device, byte control, AXRVector2D axis2D);
        [DllImport(Name)] private static extern void axr_PendInputPose(byte device, byte control, AXRVector3D position, AXRVector4D rotation);
        [DllImport(Name)] private static extern void axr_PendInputTouch2D(byte device, byte control, AXRVector2D position, byte state, bool active);
        [DllImport(Name)] private static extern bool axr_GetInputRaycastHit(byte device, byte control, ref AXRVector3D origin, ref AXRVector3D hitPosition, ref AXRVector3D hitNormal);

        private static int renderEvent(FrameType frameType, bool clearColor, bool renderOnTexture) {
            return (int)(((int)frameType << 24) + (clearColor ? RenderEventMaskClearColor : 0) + (renderOnTexture ? RenderEventMaskRenderOnTexture : 0));
        }

#else

        public static void Load() {}
        public static int Configure(int audioOutputSampleRate, bool hasInput, bool openglRenderTextureCoord) { return -1; }
        public static void Cleanup() { }
        public static void EnableCopyrightCheck(bool enable) { }
        public static void SetProfile(string profile) { }
        public static bool IsConnected() { return false; }
        public static bool IsPlaying() { return false; }
        public static void RequestConnect(string address, int port) { }
        public static void RequestDisconnect() { }
        public static void RequestPlay() { }
        public static void RequestStop() { }
        public static void RequestSendUserData(byte[] data) { }

        public static bool CheckMessageQueue(out IntPtr source, out IntPtr data, out int length) {
            source = default;
            data = default;
            length = 0;

            return false;
        }

        public static void RemoveFirstMessageFromQueue() { }
        public static void EnableNetworkTimeWarp(bool enable) { }
        public static void SetCameraOrientation(Quaternion rotation, ref int viewNumber) { }
        public static void SetCameraProjection(float[] projection) { } 
        public static void SetRenderAspect(float aspect) { }
        public static void SetOpacity(float opacity) { }
        public static bool GetVideoRenderTargetTexture(ref IntPtr texture, ref int width, ref int height) { return false; } 
        public static void PrepareRender() { }
        public static void PreRenderVideoFrame(int viewNumber) { }
        public static void RenderVideoFrame(AXRRenderCommand renderCommand, FrameType frameType, bool clearColor = true, bool renderOnTexture = false) { }
        public static void EndRenderVideoFrame() { }
        public static bool GetAudioData(float[] buffer, int length, int channels) { return false; }
        public static void BeginPendInput(ref long timestamp) { }
        public static void PendInputState(byte device, byte control, byte state) { }
        public static void PendInputByteAxis(byte device, byte control, byte axis) { }
        public static void PendInputAxis(byte device, byte control, float axis) { }
        public static void PendInputAxis2D(byte device, byte control, Vector2 axis2D) { }
        public static void PendInputPose(byte device, byte control, Vector3 position, Quaternion rotation) { }
        public static void PendInputTouch2D(byte device, byte control, Vector2 position, byte state, bool active) { }
        public static void SendPendingInputs(long timestamp) { }
        public static bool GetInputState(byte device, byte control, ref byte state) { return false; }
        public static bool GetInputRaycastHit(byte device, byte control, ref Vector3 origin, ref Vector3 hitPosition, ref Vector3 hitNormal) { return false; }
        public static bool GetInputVibration(byte device, byte control, ref float frequency, ref float amplitude) { return false; }
        public static void UpdateInputFrame() { }
        public static void ClearInput() { }

#endif
    }
}
