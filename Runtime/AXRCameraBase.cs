using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

namespace onAirXR.Client {
    public abstract class AXRCameraBase : MonoBehaviour {
        private Transform _thisTransform;
        private Camera _camera;
        private VideoRenderer _videoRenderer;
        private VolumeRenderer _volumeRenderer;
        private AXRClientAudioRenderer _audioRenderer;
        private AXRHeadTrackerInputDevice _headTracker;
        private bool _aboutToDestroy;
        private Vector2 _savedCameraClipPlanes;

        [SerializeField] private AudioMixerGroup _audioMixerGroup = null;

        protected Camera thisCamera => _camera;
        protected GameObject tinyOpaqueObject { get; private set; }

        protected abstract IAXRAnchor anchor { get; }
        protected virtual RenderTexture videoTexture => null;

        public float depth => _camera.depth;

        public abstract AXRProfileBase profile { get; }
        public abstract float deviceBatteryLevel { get; }

        protected abstract void OnRecenterPose();
        protected abstract void OnPostStart(IAXRAnchor anchor);

        public virtual void OnPreLink() {}

        protected virtual void Awake() {
            _thisTransform = transform;
            _camera = GetComponent<Camera>();
            _videoRenderer = createVideoRenderer();
            _volumeRenderer = new VolumeRenderer(_camera, profile, _videoRenderer);
            _audioRenderer = createAudioRenderer();
            _headTracker = new AXRHeadTrackerInputDevice(this, anchor, _thisTransform);

            AXRClientPlugin.Load();
        }

        protected virtual void Start() {
            runLoopOnEndOfFrame();

            // ensure camera renders more than one opaque object to guarantee native mesh renderer to work
            createTinyOpaqueObjectOntoFrustumEdge(_camera);

            AXRClient.LoadOnce(profile, this);
            AXRClient.OnMessageReceived += onAXRMessageReceived;
            AXRClient.inputManager.Register(_headTracker);

            // workaround: save for the very first disconnected event
            saveCameraClipPlanes(); 

            OnPostStart(anchor);
        }

        public virtual void PreRender(Camera camera) {
            _videoRenderer.OnPreRender(camera, _headTracker, profile);
            _volumeRenderer.OnPreRender(camera, anchor);
        }

        public virtual void PostRender(Camera camera) {
            _videoRenderer.OnPostRender(camera, _headTracker, profile);
            _volumeRenderer.OnPostRender();
        }

        protected virtual void OnDestroy() {
            _aboutToDestroy = true;

            AXRClient.inputManager.Unregister(_headTracker);
            AXRClient.OnMessageReceived -= onAXRMessageReceived;

            destroyTinyOpaqueObject();
        }

        private VideoRenderer createVideoRenderer() {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return new WindowsVideoRenderer(this, profile);
#elif !UNITY_EDITOR && UNITY_ANDROID
            return new AndroidVideoRenderer(this, profile);
#elif !UNITY_EDITOR && UNITY_IOS
            return new IOSVideoRenderer(this, profile);
#else
            return null;
#endif
        }

        private AXRClientAudioRenderer createAudioRenderer() {
            var go = new GameObject("AXRAudioRenderer");
            go.transform.SetParent(_thisTransform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            var audioSource = go.AddComponent<AudioSource>();
            audioSource.loop = false;

            if (_audioMixerGroup != null) {
                audioSource.outputAudioMixerGroup = _audioMixerGroup;

                // workaround for applying audio mixer group change
                audioSource.Stop();
                audioSource.Play();
            }

            return go.AddComponent<AXRClientAudioRenderer>();
        }

        private async void runLoopOnEndOfFrame() {
            while (_aboutToDestroy == false) {
                await Task.Yield();

                _videoRenderer?.OnEndOfFrame(_camera, _headTracker, profile);
            }
        }

        private void createTinyOpaqueObjectOntoFrustumEdge(Camera camera) {
            if (tinyOpaqueObject != null) { return; }

            var prefab = Resources.Load<GameObject>("AXRTinyOpaqueObject");
            if (prefab == null) { 
                Debug.LogWarning("[onairxr] AXRTinyOpaqueObject prefab is not found. Please make sure that the prefab exists in Resources folder.");
                return; 
            }

            tinyOpaqueObject = Instantiate(prefab, camera.transform);

            var frustum = camera.stereoEnabled && camera.stereoTargetEye == StereoTargetEyeMask.Both ? 
                camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left).decomposeProjection : 
                camera.projectionMatrix.decomposeProjection;

            var near = Mathf.Abs(frustum.zNear);
            var far = Mathf.Abs(frustum.zFar);
            var y = frustum.bottom / near * far;
            
            tinyOpaqueObject.transform.localPosition = new Vector3(0, y, far);
            tinyOpaqueObject.transform.localRotation = Quaternion.identity;
            tinyOpaqueObject.transform.localScale = Vector3.one * 0.05f * far / 100.0f; // 5cm at 100m
        }

        private void destroyTinyOpaqueObject() {
            if (tinyOpaqueObject == null) { return; }

            Destroy(tinyOpaqueObject);
            tinyOpaqueObject = null;
        }

        private void onAXRMessageReceived(AXRClientMessage message) {
            if (message.IsSessionEvent()) {
                switch (message.Name) {
                    case AXRClientMessage.NameConnected:
                        saveCameraClipPlanes();
                        AXRClientPlugin.EnableNetworkTimeWarp(true);
                        break;
                    case AXRClientMessage.NameDisconnected:
                        restoreCameraClipPlanes();
                        break;
                }
            }
            else if (message.IsMediaStreamEvent()) {
                switch (message.Name) {
                    case AXRClientMessage.NameCameraClipPlanes:
                        setCameraClipPlanes(message.NearClip, message.FarClip);
                        break;
                    case AXRClientMessage.NameEnableNetworkTimeWarp:
                        AXRClientPlugin.EnableNetworkTimeWarp(message.Enable);
                        break;
                }
            }
            else if (message.IsInputStreamEvent()) {
                switch (message.Name) {
                    case AXRClientMessage.NameRecenterPose:
                        OnRecenterPose();
                        break;
                }
            }
        }

        private void saveCameraClipPlanes() {
            _savedCameraClipPlanes.x = _camera.nearClipPlane;
            _savedCameraClipPlanes.y = _camera.farClipPlane;
        }

        private void setCameraClipPlanes(float nearClip, float farClip) {
            _camera.nearClipPlane = Mathf.Min(nearClip, _camera.nearClipPlane);
            _camera.farClipPlane = Mathf.Max(farClip, _camera.farClipPlane);
        }

        private void restoreCameraClipPlanes() {
            _camera.nearClipPlane = _savedCameraClipPlanes.x;
            _camera.farClipPlane = _savedCameraClipPlanes.y;
        }

        private abstract class VideoRenderer {
            private AXRRenderCommand _renderCommand;
            private bool _renderedOnce;

            public AXRRenderCamera renderCamera { get; private set; }

            public VideoRenderer(AXRCameraBase owner, AXRProfileBase profile) {
                var go = owner.gameObject;
                if (profile.propUseDedicatedRenderCamera) {
                    go = new GameObject("RenderCamera");
                    go.transform.SetParent(owner.transform, false);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                }
                
                renderCamera = go.AddComponent<AXRRenderCamera>();
                renderCamera.Configure(owner);
            }

            public void OnPreRender(Camera camera, AXRHeadTrackerInputDevice headTracker, AXRProfileBase profile) {
                ensureRenderCommandCreated(camera, profile);
                if ((_renderCommand is AXRCameraEventRenderCommand) == false) { return; }

                renderFrame(camera, headTracker, profile);
            }

            public void OnPostRender(Camera camera, AXRHeadTrackerInputDevice headTracker, AXRProfileBase profile) {
                if ((_renderCommand is AXRImmediateRenderCommand) == false) { 
                    _renderCommand.Clear();
                    return; 
                }

                renderFrame(camera, headTracker, profile);
            }

            public void OnEndOfFrame(Camera camera, AXRHeadTrackerInputDevice headTracker, AXRProfileBase profile) {
                if (_renderedOnce == false) {
                    ensureRenderCommandCreated(camera, profile);
                    renderFrame(camera, headTracker, profile);
                }
                else {
                    _renderedOnce = false;
                }

                OnFinishRenderFrame();
            }

            protected abstract void OnRenderFrame(AXRClientPlugin.FrameType type, 
                                                  AXRHeadTrackerInputDevice headTracker, 
                                                  AXRProfileBase profile, 
                                                  AXRRenderCommand renderCommand);
            protected abstract void OnFinishRenderFrame();

            private void ensureRenderCommandCreated(Camera camera, AXRProfileBase profile) {
                if (_renderCommand != null) { return; }

                _renderCommand = profile.useSeperateVideoRenderTarget ? 
                    new AXRImmediateRenderCommand() : 
                    new AXRCameraEventRenderCommand(camera, CameraEvent.BeforeForwardOpaque);
            }

            private void renderFrame(Camera camera, AXRHeadTrackerInputDevice headTracker, AXRProfileBase profile) {
                if (AXRClient.state == AXRClientState.Playing) {
                    if (AXRClient.volumetric) {
                        OnRenderFrame(frameType(camera, _renderedOnce), headTracker, profile, _renderCommand);
                    }
                    else {
                        AXRClient.RunRenderOnFramebufferTexture(renderCamera, _renderCommand, (renderCommand) => 
                            OnRenderFrame(frameType(camera, _renderedOnce), headTracker, profile, renderCommand)
                        );
                    }
                }
                _renderedOnce = true;
            }

            private AXRClientPlugin.FrameType frameType(Camera camera, bool renderedOnce) {
                if (camera == null) { return AXRClientPlugin.FrameType.Mono; }

                return camera.stereoEnabled && camera.stereoTargetEye == StereoTargetEyeMask.Both ?
                           (renderedOnce ? AXRClientPlugin.FrameType.StereoRight : AXRClientPlugin.FrameType.StereoLeft) :
                           AXRClientPlugin.FrameType.Mono;
            }
        }

        private class AndroidVideoRenderer : VideoRenderer {
            private int _viewNumber;

            public AndroidVideoRenderer(AXRCameraBase owner, AXRProfileBase profile) : base(owner, profile) { }

            protected override void OnRenderFrame(AXRClientPlugin.FrameType type, 
                                                  AXRHeadTrackerInputDevice headTracker, 
                                                  AXRProfileBase profile, 
                                                  AXRRenderCommand renderCommand) {
                switch (type) {
                    case AXRClientPlugin.FrameType.Mono:
                    case AXRClientPlugin.FrameType.StereoLeft:
                        AXRClientPlugin.SetCameraPose(headTracker.currentPose, ref _viewNumber);
                        AXRClientPlugin.PreRenderVideoFrame(_viewNumber);
                        AXRClientPlugin.RenderVideoFrame(renderCommand, type);
                        break;
                    case AXRClientPlugin.FrameType.StereoRight:
                        AXRClientPlugin.RenderVideoFrame(renderCommand, type, profile.useSingleTextureForEyes == false);
                        break;
                }
            }

            protected override void OnFinishRenderFrame() {
                AXRClientPlugin.EndRenderVideoFrame();
            }
        }

        private class WindowsVideoRenderer : VideoRenderer {
            public WindowsVideoRenderer(AXRCameraBase owner, AXRProfileBase profile) : base(owner, profile) { }

            protected override void OnRenderFrame(AXRClientPlugin.FrameType type, 
                                                  AXRHeadTrackerInputDevice headTracker, 
                                                  AXRProfileBase profile, 
                                                  AXRRenderCommand renderCommand) {
                // Currently, Windows supports monoscopic video only
                AXRClientPlugin.RenderVideoFrame(renderCommand, AXRClientPlugin.FrameType.Mono);
            }

            protected override void OnFinishRenderFrame() {}
        }

        private class IOSVideoRenderer : VideoRenderer {
            public IOSVideoRenderer(AXRCameraBase owner, AXRProfileBase profile) : base(owner, profile) { }

            protected override void OnRenderFrame(AXRClientPlugin.FrameType type, 
                                                  AXRHeadTrackerInputDevice headTracker, 
                                                  AXRProfileBase profile, 
                                                  AXRRenderCommand renderCommand) {
                // Currently, iOS supports monoscopic video only
                AXRClientPlugin.RenderVideoFrame(renderCommand, AXRClientPlugin.FrameType.Mono, true, true);
            }

            protected override void OnFinishRenderFrame() {}
        }

        private class VolumeRenderer {
            private VideoRenderer _videoRenderer;
            private AXRRenderCommand _renderCommand;
            private RenderData[] _renderData = new RenderData[2];
            private IntPtr[] _nativeRenderData = new IntPtr[2];
            private int _eyeIndex = 0;

            public VolumeRenderer(Camera camera, AXRProfileBase profile, VideoRenderer videoRenderer) {
                _videoRenderer = videoRenderer;
                _renderCommand = profile.useSeperateVideoRenderTarget ? new AXRCameraEventRenderCommand(camera, CameraEvent.BeforeForwardOpaque) :
                                                                        new AXRCameraEventRenderCommand(camera, CameraEvent.AfterForwardOpaque);
            }

            public void OnPreRender(Camera camera, IAXRAnchor anchor) {
                ensureNativeRenderDataAllocated(_eyeIndex);

                _renderData[_eyeIndex].Update(camera, _eyeIndex, anchor);
                Marshal.StructureToPtr(_renderData[_eyeIndex], _nativeRenderData[_eyeIndex], true);

                var stereoscopic = camera.stereoEnabled && camera.stereoTargetEye == StereoTargetEyeMask.Both;
                var frameType = stereoscopic == false ? AXRClientPlugin.FrameType.Mono :
                                _eyeIndex == 1 ? AXRClientPlugin.FrameType.StereoRight : AXRClientPlugin.FrameType.StereoLeft;

                AXRClient.RunRenderOnFramebufferTexture(_videoRenderer.renderCamera, _renderCommand, (renderCommand) =>
                    AXRClientPlugin.RenderVolume(renderCommand, frameType, _nativeRenderData[_eyeIndex])
                );

                if (stereoscopic) {
                    _eyeIndex = 1 - _eyeIndex;
                }
            }

            public void OnPostRender() {
                _renderCommand.Clear();
            }

            public void Cleanup() {
                for (var index = 0; index < 2; index++) {
                    if (_nativeRenderData[index] == IntPtr.Zero) { continue; }

                    Marshal.FreeHGlobal(_nativeRenderData[index]);
                    _nativeRenderData[index] = IntPtr.Zero;
                }
            }

            private void ensureNativeRenderDataAllocated(int eyeIndex) {
                if (_nativeRenderData[eyeIndex] != IntPtr.Zero) { return; }

                _nativeRenderData[eyeIndex] = Marshal.AllocHGlobal(RenderData.Size);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RenderData {
            public static readonly int Size = sizeof(float) * 16 * 2;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public float[] anchorViewMatrix;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public float[] projectionMatrix;

            public void Update(Camera camera, int eyeIndex, IAXRAnchor anchor) {
                ensureMemoryAlocated();

                var eye = eyeIndex == 1 ? Camera.StereoscopicEye.Right : Camera.StereoscopicEye.Left;
                var worldToAnchor = anchor?.worldToAnchorMatrix ?? Matrix4x4.identity;
                var viewMatrix = camera.stereoEnabled ? camera.GetStereoViewMatrix(eye) : camera.transform.worldToLocalMatrix;
                var projMatrix = camera.stereoEnabled ? camera.GetStereoProjectionMatrix(eye) : camera.projectionMatrix;
                var anchorToView = viewMatrix * worldToAnchor.inverse;
                var projection = GL.GetGPUProjectionMatrix(projMatrix, true);
                
                writeMatrixToFloatArray(anchorToView, ref anchorViewMatrix);
                writeMatrixToFloatArray(projection, ref projectionMatrix);
            }

            private void ensureMemoryAlocated() {
                if (anchorViewMatrix == null) {
                    anchorViewMatrix = new float[16];
                }
                if (projectionMatrix == null) {
                    projectionMatrix = new float[16];
                }
            }

            private void writeMatrixToFloatArray(Matrix4x4 mat, ref float[] dest) {
                dest[0] = mat.m00;
                dest[1] = mat.m10;
                dest[2] = mat.m20;
                dest[3] = mat.m30;

                dest[4] = mat.m01;
                dest[5] = mat.m11;
                dest[6] = mat.m21;
                dest[7] = mat.m31;

                dest[8] = mat.m02;
                dest[9] = mat.m12;
                dest[10] = mat.m22;
                dest[11] = mat.m32;

                dest[12] = mat.m03;
                dest[13] = mat.m13;
                dest[14] = mat.m23;
                dest[15] = mat.m33;
            }
        }
    }
}
