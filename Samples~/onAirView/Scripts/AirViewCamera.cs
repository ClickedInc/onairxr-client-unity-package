/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using onAirXR.Client;

public class AirViewCamera : MonoBehaviour {
    private Transform _thisTransform;
    private Camera _camera;
    private AirViewMotionTrackerInputDevice _motionTracker;
    private VideoRenderer _videoRenderer;
    private bool _aboutToDestroy;

    [SerializeField] private bool _sendScreenTouches = true;
    [SerializeField] private AirViewProfile.VideoBitrate _videoBitrate = AirViewProfile.VideoBitrate.Normal;
    [SerializeField] private RenderTexture _targetTexture = null;
    [SerializeField] private bool _forceStereoscopicInEditor = false;
    [SerializeField] private string _videoCodecInEditor = "H264";

    [SerializeField] private Transform _manualFollowTransform = null;
    [SerializeField, Range(0.01f, 179.9f)] private float _manualFieldOfView = 60.0f;
    [SerializeField, Min(0.0f)] private float _manualAspectRatio = 16.0f / 9.0f;

    private Transform followTransform => _camera ? _thisTransform : _manualFollowTransform;
    private float defaultAspect => _camera ? _camera.aspect : _manualAspectRatio;

    internal Pose pose => _motionTracker.currentPose;
    internal (int width, int height) viewportSize => _camera == null && _targetTexture ? (_targetTexture.width, _targetTexture.height) : 
                                                                                         (Display.main.renderingWidth, Display.main.renderingHeight);

    internal bool stereoscopic => Application.isEditor && _forceStereoscopicInEditor;
    internal string[] videoCodecs => Application.isEditor ? new string[] { _videoCodecInEditor == "H265" ? "H265" : "H264" } : null;

    internal float[] defaultProjection {
        get {
            if (_camera) {
                var inverse = _camera.projectionMatrix.inverse;
                var leftTop = inverse.MultiplyPoint(Vector3.left + Vector3.up + Vector3.back) / _camera.nearClipPlane;
                var rightBottom = inverse.MultiplyPoint(Vector3.right + Vector3.down + Vector3.back) / _camera.nearClipPlane;

                return new float[] { 
                    leftTop.x / (stereoscopic ? 2 : 1), 
                    leftTop.y, 
                    rightBottom.x / (stereoscopic ? 2 : 1), 
                    rightBottom.y 
                };
            }
            else {
                var top = Mathf.Tan(_manualFieldOfView * Mathf.Deg2Rad / 2);
                var right = top * _manualAspectRatio;

                return new float[] { 
                    -right / (stereoscopic ? 2 : 1), 
                    top, 
                    right / (stereoscopic ? 2 : 1), 
                    -top 
                };
            }
        }
    }

    public AirViewProfile profile { get; private set; }
    public AirViewTrackingSpace trackingSpace { get; private set; }

    public void SetProjection(float fieldOfView) {
        if (fieldOfView <= 0.0f || fieldOfView >= Mathf.PI) {
            Debug.LogErrorFormat("[ERROR] field of view must be in (0, PI) : {0}", fieldOfView);
            return;
        }

        var halfHeight = Mathf.Tan(fieldOfView / 2);
        var halfWidth = halfHeight * defaultAspect;

        profile.cameraProjection = new float[] { 
            -halfWidth / (stereoscopic ? 2 : 1), 
            halfHeight, 
            halfWidth / (stereoscopic ? 2 : 1),
            -halfHeight 
        };

        if (AirViewClient.connected) {
            AXRClientPlugin.SetCameraProjection(profile.cameraProjection);
        }
    }

    public void SetProjection(float[] projection) {
        if (projection != null && projection.Length != 4) {
            Debug.LogError("[ERROR] projection must be an array of 4 floats, or null.");
            return;
        }

        profile.cameraProjection = projection;

        if (AirViewClient.connected) {
            AXRClientPlugin.SetCameraProjection(profile.cameraProjection);
        }
    }

    public void SetAspectRatio(float aspect) {
        if (aspect < 0) {
            Debug.LogErrorFormat("[ERROR] aspect ratio must be not less than zero : {0}", aspect);
            return;
        }
        else if (aspect == 0) {
            AXRClientPlugin.SetRenderAspect(1.0f);
        }

        // render aspect is relative to the full aspect
        AXRClientPlugin.SetRenderAspect(aspect / defaultAspect);
    }

    private void Awake() {
        _thisTransform = transform;
        _camera = GetComponent<Camera>();

        _motionTracker = new AirViewMotionTrackerInputDevice();
        _motionTracker.tracker = followTransform;

        profile = new AirViewProfile(this, _videoBitrate);
        trackingSpace = new AirViewTrackingSpace(this);

        AXRClientPlugin.Load();

        createVideoRenderer();
        createAudioRenderer();

        if (Application.isEditor) {
            var playerController = gameObject.GetComponent<AirViewStandardInputPlayerController>();
            if (playerController == null) {
                gameObject.AddComponent<AirViewStandardInputPlayerController>();
            }
        }
    }

    private void Start() {
        AirViewClient.MessageReceived += onAirViewClientMessageReceived;
        runLoopOnEndOfFrame();

        AirViewClient.LoadOnce(this);

        AirViewClient.input.RegisterInputSender(_motionTracker);
        if (_sendScreenTouches) {
            AirViewClient.input.RegisterInputSender(new AirViewTouchScreenInputDevice());
        }

        _motionTracker.SetTrackingSpace(trackingSpace);
    }

    private void Update() {
        trackingSpace.Update();
    }

    private void OnPreRender() {
        _videoRenderer?.OnPreRender(followTransform, _camera, _targetTexture);
    }

    private void OnPostRender() {
        _videoRenderer?.OnPostRender(followTransform, _camera, _targetTexture);
    }

    private void OnDestroy() {
        _aboutToDestroy = true;
        AirViewClient.MessageReceived -= onAirViewClientMessageReceived;
    }

    private async void runLoopOnEndOfFrame() {
        while (_aboutToDestroy == false) {
            await Task.Yield();

            _videoRenderer?.OnEndOfFrame(followTransform, _camera, _targetTexture);
        }
    }

    private void createVideoRenderer() {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        _videoRenderer = new WindowsVideoRenderer();
#elif !UNITY_EDITOR && UNITY_ANDROID
        _videoRenderer = new AndroidVideoRenderer();
#elif !UNITY_EDITOR && UNITY_IOS
        _videoRenderer = new IOSVideoRenderer(this, _targetTexture, _manualFieldOfView, _manualAspectRatio);
#else
        _videoRenderer = null;
#endif
    }

    private void onAirViewClientMessageReceived(AXRMessage message) {
        var clientmsg = message as AirViewMessage;

        if (clientmsg.IsSessionEvent()) {
            if (clientmsg.Name.Equals(AirViewMessage.NameConnected)) {
                //OnSessionConnected();
            }
            else if (clientmsg.Name.Equals(AirViewMessage.NameDisconnected)) {
                //OnSessionDisconnected();
            }
        }
    }

    private void createAudioRenderer() {
        var go = new GameObject("AudioRenderer");
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        var audioSource = go.AddComponent<AudioSource>();
        audioSource.loop = false;

        go.AddComponent<AirViewAudioRenderer>();
    }

    private abstract class VideoRenderer {
        public abstract void OnPreRender(Transform transform, Camera camera, RenderTexture targetTexture);
        public abstract void OnPostRender(Transform transform, Camera camera, RenderTexture targetTexture);
        public abstract void OnEndOfFrame(Transform transform, Camera camera, RenderTexture targetTexture);
    }

    private abstract class DefaultVideoRenderer : VideoRenderer {
        private AXRRenderCommand _renderCommand;
        private bool _renderedOnce;

        public override void OnPreRender(Transform transform, Camera camera, RenderTexture targetTexture) {
            ensureRenderCommandCreated(camera, targetTexture);
            if ((_renderCommand is AXRCameraEventRenderCommand) == false) { return; }

            renderFrame(camera, transform, targetTexture);
        }

        public override void OnPostRender(Transform transform, Camera camera, RenderTexture targetTexture) {
            if ((_renderCommand is AXRImmediateRenderCommand) == false) {
                _renderCommand.Clear();
                return;
            }

            renderFrame(camera, transform, targetTexture);
        }

        public override void OnEndOfFrame(Transform transform, Camera camera, RenderTexture targetTexture) {
            if (_renderedOnce == false) {
                ensureRenderCommandCreated(camera, targetTexture);
                renderFrame(camera, transform, targetTexture);
            }
            else {
                _renderedOnce = false;
            }

            OnFinishRenderFrame();
        }

        public abstract void OnRenderFrame(AXRClientPlugin.FrameType type, Transform transform, AXRRenderCommand renderCommand);
        public abstract void OnFinishRenderFrame();

        private void ensureRenderCommandCreated(Camera camera, RenderTexture targetTexture) {
            if (_renderCommand != null) { return; }

            _renderCommand = targetTexture == null && camera != null ?
                new AXRCameraEventRenderCommand(camera, CameraEvent.BeforeForwardOpaque) as AXRRenderCommand :
                new AXRImmediateRenderCommand() as AXRRenderCommand;
        }

        private void renderFrame(Camera camera, Transform transform, RenderTexture targetTexture) {
            if (AirViewClient.playing) {
                var type = frameType(camera, _renderedOnce);
                var renderOntoTexture = camera == null && targetTexture && _renderedOnce == false;

                RenderTexture prevActiveTexture = null;
                if (renderOntoTexture) {
                    prevActiveTexture = RenderTexture.active;
                    RenderTexture.active = targetTexture;
                }

                OnRenderFrame(frameType(camera, _renderedOnce), transform, _renderCommand);

                if (renderOntoTexture) {
                    RenderTexture.active = prevActiveTexture;
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

    private class AndroidVideoRenderer : DefaultVideoRenderer {
        private int _viewNumber;
        private Quaternion _lastOrientation = Quaternion.identity;

        public override void OnRenderFrame(AXRClientPlugin.FrameType type, Transform transform, AXRRenderCommand renderCommand) {
            switch (type) {
                case AXRClientPlugin.FrameType.StereoLeft:
                    _lastOrientation = transform?.localRotation ?? _lastOrientation;

                    AXRClientPlugin.SetCameraOrientation(_lastOrientation, ref _viewNumber);
                    AXRClientPlugin.PreRenderVideoFrame(_viewNumber);

                    AXRClientPlugin.RenderVideoFrame(renderCommand, type);
                    break;
                case AXRClientPlugin.FrameType.StereoRight:
                    AXRClientPlugin.RenderVideoFrame(renderCommand, type);
                    break;
                case AXRClientPlugin.FrameType.Mono:
                    AXRClientPlugin.SetCameraOrientation(transform.localRotation, ref _viewNumber);
                    AXRClientPlugin.PreRenderVideoFrame(_viewNumber);
                    AXRClientPlugin.RenderVideoFrame(renderCommand, type);
                    break;
            }
        }

        public override void OnFinishRenderFrame() {
            AXRClientPlugin.EndRenderVideoFrame();
        }
    }

    private class WindowsVideoRenderer : DefaultVideoRenderer {
        public override void OnRenderFrame(AXRClientPlugin.FrameType type, Transform transform, AXRRenderCommand renderCommand) {
            // Currently, Windows supports monoscopic video only
            AXRClientPlugin.RenderVideoFrame(renderCommand, AXRClientPlugin.FrameType.Mono);
        }

        public override void OnFinishRenderFrame() {
            // do nothing
        }
    }

    private class IOSVideoRenderer : VideoRenderer {
        private AXRRenderCommand _renderCommand = new AXRImmediateRenderCommand();

        public IOSVideoRenderer(AirViewCamera owner, RenderTexture targetTexture, float manualFov, float manualAspect) {
            if (owner._camera == null) {
                if (targetTexture == null) {
                    throw new UnityException("[ERROR] there is a limitation on iOS that target texture must not be null.");
                }

                var camera = owner.gameObject.AddComponent<Camera>();

                camera.fieldOfView = manualFov;
                camera.aspect = manualAspect;
                camera.targetTexture = targetTexture;

                camera.clearFlags = CameraClearFlags.Color;
                camera.backgroundColor = Color.black;
                camera.nearClipPlane = 0.0001f;
                camera.farClipPlane = 0.0002f;
            }
        }

        public override void OnPreRender(Transform transform, Camera camera, RenderTexture targetTexture) {
            // do nothing
        }

        public override void OnPostRender(Transform transform, Camera camera, RenderTexture targetTexture) {
            if (AirViewClient.playing) {
                AXRClientPlugin.RenderVideoFrame(_renderCommand,
                                                 AXRClientPlugin.FrameType.Mono,
                                                 true,
                                                 (camera == null && targetTexture != null) ||
                                                 (camera != null && camera.targetTexture != null));
            }
        }

        public override void OnEndOfFrame(Transform transform, Camera camera, RenderTexture targetTexture) {
            // do nothing
        }
    }
}
