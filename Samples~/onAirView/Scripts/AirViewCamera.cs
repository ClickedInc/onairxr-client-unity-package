/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using onAirXR.Client;

public class AirViewCamera : AXRCameraBase {
    private AirViewProfile _profile;
    private ARCameraBackground _cameraBackground;
    private AirViewTouchScreenInputDevice _touchScreenInputDevice;

    [SerializeField] private AXRAnchorComponent _anchor = null;
    [SerializeField] private bool _sendScreenTouches = false;
    [SerializeField] private bool _forceStereoscopicInEditor = false;
    [SerializeField] private string _videoCodecInEditor = "H264";

    private float defaultAspect => thisCamera.aspect;

    public (int width, int height) viewportSize => (Display.main.renderingWidth, Display.main.renderingHeight);
    public bool stereoscopic => Application.isEditor && _forceStereoscopicInEditor;
    public string[] videoCodecs => Application.isEditor ? new string[] { _videoCodecInEditor == "H265" ? "H265" : "H264" } : null;

    public float[] defaultProjection {
        get {
            var inverse = thisCamera.projectionMatrix.inverse;
            var leftTop = inverse.MultiplyPoint(Vector3.left + Vector3.up + Vector3.back) / thisCamera.nearClipPlane;
            var rightBottom = inverse.MultiplyPoint(Vector3.right + Vector3.down + Vector3.back) / thisCamera.nearClipPlane;

            return new float[] { 
                leftTop.x / (stereoscopic ? 2 : 1), 
                leftTop.y, 
                rightBottom.x / (stereoscopic ? 2 : 1), 
                rightBottom.y 
            };
        }
    }

    public void SetProjection(float vfov) {
        if (vfov <= 0.0f || vfov >= Mathf.PI) {
            Debug.LogErrorFormat("[onairxr] ERROR: field of view must be in (0, PI) : {0}", vfov);
            return;
        }

        var halfHeight = Mathf.Tan(vfov / 2);
        var halfWidth = halfHeight * defaultAspect;

        SetProjection(new float[] { 
            -halfWidth / (stereoscopic ? 2 : 1), 
            halfHeight, 
            halfWidth / (stereoscopic ? 2 : 1),
            -halfHeight 
        });
    }

    public void SetProjection(float[] projection) {
        if (projection != null && projection.Length != 4) {
            Debug.LogError("[onairxr] ERROR: projection must be an array of 4 floats, or null.");
            return;
        }

        _profile.SetCameraProjection(projection);

        if (AXRClient.connected) {
            AXRClientPlugin.SetCameraProjection(projection);
        }
    }

    public void SetAspectRatio(float aspect) {
        if (aspect < 0) {
            Debug.LogErrorFormat("[onairxr] ERROR: aspect ratio must be not less than zero : {0}", aspect);
            return;
        }
        else if (aspect == 0) {
            AXRClientPlugin.SetRenderAspect(1.0f);
        }

        // render aspect is relative to the full aspect
        AXRClientPlugin.SetRenderAspect(aspect / defaultAspect);
    }

    protected override void Awake() {
        _profile = new AirViewProfile(this);

        base.Awake();

        if (Application.isEditor) {
            var playerController = gameObject.GetComponent<AirViewStandardInputPlayerController>();
            if (playerController == null) {
                gameObject.AddComponent<AirViewStandardInputPlayerController>();
            }
        }
    }

    // implements AXRCameraBase
    protected override IAXRAnchor anchor => _anchor;
    public override AXRProfileBase profile => _profile;
    public override float deviceBatteryLevel => SystemInfo.batteryLevel;

    protected override void OnRecenterPose() {
        // do nothing for now
    }

    protected override void OnPostStart(IAXRAnchor anchor) {
        if (_sendScreenTouches) {
            _touchScreenInputDevice = new AirViewTouchScreenInputDevice();

            AXRClient.inputManager.Register(_touchScreenInputDevice);
        }
    }
}
