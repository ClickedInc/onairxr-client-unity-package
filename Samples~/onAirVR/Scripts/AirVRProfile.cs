/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

public class AirVRProfile : AXRProfileBase {
    protected override (int width, int height) defaultVideoResolution {
        get {
#if UNITY_EDITOR || UNITY_STANDALONE
            return (2048, 2048);
#else
            // NOTE: hardcoded for meta quest 2 & pro
            return (3200, 1600);
#endif
        }
    }

    // NOTE: use 90 fps as upper limit for now
    protected override float defaultVideoFrameRate => Application.isEditor ? 60.0f : Mathf.Min(OVRManager.display?.displayFrequency ?? 60.0f, 90.0f);
    protected override bool stereoscopy => true;
    protected override RenderType renderType => RenderType.DirectOnFrameBufferTexture;
    protected override bool isUserPresent => OVRManager.instance.isUserPresent;
    protected override bool isOpenglRenderTextureCoordInEditor => true;

    protected override float[] leftEyeCameraNearPlane {
        get {
            try {
                // workaround : GetEyeRenderDesc returns wrong fullFov due to incorrect AsymmetricFovEnabled value
                if (OVRPlugin.GetNodeFrustum2(OVRPlugin.Node.EyeLeft, out var frust)) {
                    return new float[] {
                        -frust.Fov.LeftTan,
                        frust.Fov.UpTan,
                        frust.Fov.RightTan,
                        -frust.Fov.DownTan
                    };
                }

                var desc = OVRManager.display.GetEyeRenderDesc(UnityEngine.XR.XRNode.LeftEye);
                return new float[] {
                    -Mathf.Tan(desc.fullFov.LeftFov / 180.0f * Mathf.PI),
                    Mathf.Tan(desc.fullFov.UpFov / 180.0f * Mathf.PI),
                    Mathf.Tan(desc.fullFov.RightFov / 180.0f * Mathf.PI),
                    -Mathf.Tan(desc.fullFov.DownFov / 180.0f * Mathf.PI),
                };    
            }
            catch (System.Exception) {
                return new float[] { -1f, 1f, 1f, -1f };
            }
        }
    }

    protected override float ipd => OVRManager.profile.ipd;

    protected override int[] leftEyeViewport {
        get {
            if (Application.isEditor) {
                return new int[] { 0, 0, Display.main.renderingWidth, Display.main.renderingHeight };
            }

            var desc = OVRManager.display.GetEyeRenderDesc(UnityEngine.XR.XRNode.LeftEye);
			return new int[] { 0, 0, (int)desc.resolution.x, (int)desc.resolution.y };
        }
    }

    protected override int[] rightEyeViewport => leftEyeViewport;

    // deprecated
    protected override Vector3 eyeCenterPosition {
        get {
            return new Vector3(0.0f, OVRManager.profile.eyeHeight - OVRManager.profile.neckHeight, OVRManager.profile.eyeDepth);
        }
    }
}
