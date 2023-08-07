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
    private float[] _explicitCameraProjection;
    private float[] _implicitCameraProjection;

    public AirViewProfile(AirViewCamera owner) {
        _owner = owner;
    }

    // implements AXRProfileBase
    protected override (int width, int height) defaultVideoResolution => _owner.viewportSize;

    protected override float defaultVideoFrameRate {
        get {
            if (Application.isEditor) { 
                return 60.0f;
            }
            else if (Application.platform == RuntimePlatform.Android) { 
                return 30.0f; 
            }
            else {
                return Mathf.Min(AXRClientPlugin.GetOptimisticVideoFrameRate(), 60.0f);
            }
        }
    }

    protected override bool stereoscopy => _owner.stereoscopic;
    protected override RenderType renderType => RenderType.VideoRenderTextureInScene;
    protected override string[] supportedVideoCodecs => _owner.videoCodecs ?? base.supportedVideoCodecs;
    protected override bool useDedicatedRenderCamera => Application.isEditor == false && Application.platform == RuntimePlatform.IPhonePlayer;

    protected override float[] cameraProjection {
        get {
            if (_explicitCameraProjection != null) { return _explicitCameraProjection; }

            _implicitCameraProjection = _owner.defaultProjection;
            return _implicitCameraProjection;
        }
    }

    protected override int[] renderViewport {
        get {
            var size = _owner.viewportSize;
            return new int[] { 0, 0, size.width, size.height };
        }
    }

    protected override float[] leftEyeCameraNearPlane => cameraProjection;
    protected override float ipd => 0.06f;
    protected override int[] leftEyeViewport => renderViewport;
    protected override int[] rightEyeViewport => renderViewport;

    public float[] GetCameraProjection() {
        if (_explicitCameraProjection != null) { return _explicitCameraProjection; }
        else if (_implicitCameraProjection != null) { return _implicitCameraProjection; }

        return cameraProjection;
    }

    public void SetCameraProjection(float[] projection) {
        if (projection != null && projection.Length != 4) {
            Debug.LogError("[ERROR] camera projection must be an array of 4 floats.");
            return;
        }

        _explicitCameraProjection = projection;
    }
}
