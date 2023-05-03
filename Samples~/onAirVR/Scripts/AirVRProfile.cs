/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

public class AirVRProfile : AirVRProfileBase {
    public AirVRProfile(VideoBitrate bitrate) : base(bitrate) {}

	private bool _userPresent;

    public override (int width, int height) defaultVideoResolution {
        get {
#if UNITY_EDITOR || UNITY_STANDALONE
            return (2048, 2048);
#else
            return (3200, 1600);
#endif
        }
    }

    public override float defaultVideoFrameRate => Mathf.Min(OVRPlugin.systemDisplayFrequency, 72.0f);  // fix for oculus quest 2
    public override bool stereoscopy => true;
    public override float ipd => OVRManager.profile.ipd;
    public override bool hasInput => true;
    public override RenderType renderType => RenderType.DirectOnTwoEyeTextures;
    public override bool isUserPresent => OVRManager.instance.isUserPresent;
    public override float delayToResumePlayback => 1.5f;

    public override float[] leftEyeCameraNearPlane { 
        get {
            // workaround : GetEyeRenderDesc returns wrong fullFov due to incorrect AsymmetricFovEnabled value
            OVRPlugin.Frustumf2 frust;
            if (OVRPlugin.GetNodeFrustum2(OVRPlugin.Node.EyeLeft, out frust)) {
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
    }

    public override Vector3 eyeCenterPosition { 
        get {
            return new Vector3(0.0f, OVRManager.profile.eyeHeight - OVRManager.profile.neckHeight, OVRManager.profile.eyeDepth);
        }
    }

	public override int[] leftEyeViewport { 
		get {
            //return new int[] { 0, 0, 1600, 1600 };

			OVRDisplay.EyeRenderDesc desc = OVRManager.display.GetEyeRenderDesc(UnityEngine.XR.XRNode.LeftEye);
			return new int[] { 0, 0, (int)desc.resolution.x, (int)desc.resolution.y };
		}
	}

	public override int[] rightEyeViewport { 
		get {
			return leftEyeViewport;
		}
	}

	public override float[] videoScale {
		get {
            //OVRDisplay.EyeRenderDesc desc = OVRManager.display.GetEyeRenderDesc(UnityEngine.XR.XRNode.LeftEye);
            //return new float[] { (float)videoWidth / 2 / desc.resolution.x, (float)videoHeight / desc.resolution.y };
            return new float[] { 1.0f, 1.0f };
		}
	}
}
