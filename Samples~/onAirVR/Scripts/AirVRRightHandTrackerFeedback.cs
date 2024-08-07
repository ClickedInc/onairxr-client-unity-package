﻿/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

public class AirVRRightHandTrackerFeedback : AirVRTrackerFeedback {
    // implements AirVRTrackerFeedback
    protected override AXRInputDeviceID srcDevice => AXRInputDeviceID.RightHandTracker;
    protected override OVRInput.Controller ovrController => OVRInput.Controller.RTouch;
}
