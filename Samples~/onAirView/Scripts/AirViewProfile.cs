/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;
using UnityEngine;
using onAirXR.Client;

public class AirViewProfile : AXRProfileBase {
    private AirViewCamera _owner;
    private float[] _cameraProjection;

    public AirViewProfile(AirViewCamera owner) {
        _owner = owner;
    }

    // implements AXRProfileBase
    protected override string[] supportedVideoCodecs => _owner.videoCodecs ?? base.supportedVideoCodecs;

    public override (int width, int height) defaultVideoResolution => _owner.viewportSize;

    public override float defaultVideoFrameRate {
        get {
            if (Application.isEditor) { 
                return 60.0f;
            }
            else if (Application.platform == RuntimePlatform.Android) { 
                return 30.0f; 
            }
            else {
                return Mathf.Max(AXRClientPlugin.GetOptimisticVideoFrameRate(), 60.0f);
            }
        }
    }

    public override bool stereoscopy => _owner.stereoscopic;
    public override RenderType renderType => RenderType.VideoRenderTextureInScene;
    public override bool useDedicatedRenderCamera => Application.isEditor == false && Application.platform == RuntimePlatform.IPhonePlayer;

    public override float[] cameraProjection => _cameraProjection ?? _owner.defaultProjection;

    public override int[] renderViewport {
        get {
            var size = _owner.viewportSize;
            return new int[] { 0, 0, size.width, size.height };
        }
    }

    public override float[] leftEyeCameraNearPlane => cameraProjection;
    public override float ipd => 0.06f;
    public override int[] leftEyeViewport => renderViewport;
    public override int[] rightEyeViewport => renderViewport;

    public void SetCameraProjection(float[] projection) {
        if (projection != null && projection.Length != 4) {
            Debug.LogError("[ERROR] camera projection must be an array of 4 floats.");
            return;
        }

        _cameraProjection = projection;
    }
}

/* [Serializable]
public class AirViewProfile {
    public enum ProfilerMask : int {
        Frame = 0x01,
        Report = 0x02
    }

    public enum VideoBitrate {
        Low,
        Normal,
        High,
        Best
    }

    private AirViewCamera _owner;
    private float[] _cameraProjection;

#pragma warning disable CS0414
    [SerializeField] private bool Stereoscopy;

    [SerializeField] private string[] SupportedVideoCodecs;
    [SerializeField] private string[] SupportedAudioCodecs;
    [SerializeField] private int VideoWidth;
    [SerializeField] private int VideoHeight;
    [SerializeField] private float[] VideoScale;

    [SerializeField] private string UserID;
    [SerializeField] private string TempPath;
    [SerializeField] private int VideoMinBitrate;
    [SerializeField] private int VideoStartBitrate;
    [SerializeField] private int VideoMaxBitrate;
    [SerializeField] private float VideoFrameRate;

    [SerializeField] private float[] CameraProjection;
    [SerializeField] private int[] RenderViewport;

    [SerializeField] private float IPD;
    [SerializeField] private int[] LeftEyeViewport;
    [SerializeField] private int[] RightEyeViewport;
    [SerializeField] private Vector3 EyeCenterPosition;
#pragma warning restore CS0414

    public AirViewProfile(AirViewCamera owner, VideoBitrate bitrate) {
        _owner = owner;

        switch (bitrate) {
            case VideoBitrate.Low:
                videoMinBitrate = 4000000;
                videoStartBitrate = 5000000;
                videoMaxBitrate = 6000000;
                break;
            case VideoBitrate.Normal:
                videoMinBitrate = 4000000;
                videoStartBitrate = 8000000;
                videoMaxBitrate = 12000000;
                break;
            case VideoBitrate.High:
                videoMinBitrate = 6000000;
                videoStartBitrate = 12000000;
                videoMaxBitrate = 24000000;
                break;
            case VideoBitrate.Best:
                videoMinBitrate = 8000000;
                videoStartBitrate = 16000000;
                videoMaxBitrate = 32000000;
                break;
            default:
                break;
        }

        IPD = 0.006f;
        LeftEyeViewport = new int[] { 0, 0, 1, 1 };
        RightEyeViewport = new int[] { 0, 0, 1, 1 };

        TempPath = Application.persistentDataPath;
    }

    private (int width, int height) defaultVideoResolution => _owner.viewportSize;
    private float defaultVideoFrameRate => AXRClientPlugin.GetOptimisticVideoFrameRate();
    private string[] defaultVideoCodecs => _owner.videoCodecs ?? AXRClientPlugin.GetSupportedVideoCodecs();
    private string[] defaultAudioCodecs => AXRClientPlugin.GetSupportedAudioCodecs();
    private float[] videoScale => new float[] { 1.0f, 1.0f };
    public bool stereoscopic => _owner.stereoscopic;

    public int[] renderViewport {
        get {
            var size = _owner.viewportSize;
            return new int[] { 0, 0, size.width, size.height };
        }
    }

    public string userID {
        get { return UserID; }
        set { UserID = value; }
    }

    public (int width, int height) videoResolution {
        get { return (VideoWidth, VideoHeight); }
        set {
            VideoWidth = value.width;
            VideoHeight = value.height;
        }
    }

    public int videoMinBitrate {
        get { return VideoMinBitrate; }
        set { VideoMinBitrate = value; }
    }

    public int videoStartBitrate {
        get { return VideoStartBitrate; }
        set { VideoStartBitrate = value; }
    }

    public int videoMaxBitrate {
        get { return VideoMaxBitrate; }
        set { VideoMaxBitrate = value; }
    }

    public float videoFrameRate {
        get { return VideoFrameRate; }
        set { VideoFrameRate = value; }
    }

    public string[] videoCodecs {
        get { return SupportedVideoCodecs; }
        set { SupportedVideoCodecs = value; }
    }

    public string[] audioCodecs {
        get { return SupportedAudioCodecs; }
        set { SupportedAudioCodecs = value; }
    }

    public float[] cameraProjection {
        get {
            return _cameraProjection ?? _owner.defaultProjection;
        }
        set {
            if (value != null && value.Length != 4) {
                Debug.LogError("[ERROR] camera projection must be an array of 4 floats.");
                return;
            }

            _cameraProjection = value;
        }
    } 

    public virtual string Serialize() {
        update();

        return JsonUtility.ToJson(this);
    }

    private void update() {
        Stereoscopy = stereoscopic;
        VideoScale = videoScale;
        RenderViewport = renderViewport;

        if (VideoWidth <= 0 || VideoHeight <= 0) {
            var res = defaultVideoResolution;
            VideoWidth = res.width;
            VideoHeight = res.height;
        }
        if (VideoFrameRate <= 0) {
            VideoFrameRate = defaultVideoFrameRate;
        }
        if (SupportedVideoCodecs == null || SupportedVideoCodecs.Length == 0) {
            SupportedVideoCodecs = defaultVideoCodecs;
        }
        if (SupportedAudioCodecs == null || SupportedAudioCodecs.Length == 0) {
            SupportedAudioCodecs = defaultAudioCodecs;
        }

        CameraProjection = cameraProjection;
    }
}
 */