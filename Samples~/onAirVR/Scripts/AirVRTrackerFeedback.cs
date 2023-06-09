/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

public abstract class AirVRTrackerFeedback : AXRInputDeviceFeedbackBase {
    protected abstract OVRInput.Controller ovrController { get; }

    protected override bool srcDeviceConnected => AirVROVRInputHelper.IsConnected(ovrController);

    protected override void SetVibration(float frequency, float amplitude) {
        OVRInput.SetControllerVibration(Mathf.Clamp(frequency, 0, 1.0f), Mathf.Clamp(amplitude, 0, 1.0f), ovrController);
    }
}
