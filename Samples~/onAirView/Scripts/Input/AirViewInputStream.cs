﻿/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

public class AirViewInputStream : AXRInputStream {
    // implements AXRInputStream
    protected override float maxSendingRatePerSec => 120.0f;

    protected override void BeginPendInputImpl(ref long timestamp) {
        AXRClientPlugin.BeginPendInput(ref timestamp);
    }

    protected override void PendStateImpl(byte device, byte control, byte state) {}
    protected override void PendByteAxisImpl(byte device, byte control, byte axis) { }
    protected override void PendAxisImpl(byte device, byte control, float axis) { }
    protected override void PendAxis2DImpl(byte device, byte control, Vector2 axis2D) { }

    protected override void PendPoseImpl(byte device, byte control, Vector3 position, Quaternion rotation) {
        AXRClientPlugin.PendInputPose(device, control, position, rotation);
    }

    protected override void PendRaycastHitImpl(byte device, byte control, Vector3 origin, Vector3 hitPosition, Vector3 hitNormal) { }
    protected override void PendVibrationImpl(byte device, byte control, float frequency, float amplitude) { }

    protected override void PendTouch2DImpl(byte device, byte control, Vector2 position, byte state, bool active) {
        AXRClientPlugin.PendInputTouch2D(device, control, position, state, active);
    }

    protected override void SendPendingInputEventsImpl(long timestamp) {
        AXRClientPlugin.SendPendingInputs(timestamp);
    }

    protected override bool GetStateImpl(byte device, byte control, ref byte state) { return false; }
    protected override bool GetByteAxisImpl(byte device, byte control, ref byte axis) { return false; }
    protected override bool GetAxisImpl(byte device, byte control, ref float axis) { return false; }
    protected override bool GetAxis2DImpl(byte device, byte control, ref Vector2 axis2D) { return false; }
    protected override bool GetPoseImpl(byte device, byte control, ref Vector3 position, ref Quaternion rotation) { return false; }
    protected override bool GetRaycastHitImpl(byte device, byte control, ref Vector3 origin, ref Vector3 hitPosition, ref Vector3 hitNormal) { return false; }
    protected override bool GetVibrationImpl(byte device, byte control, ref float frequency, ref float amplitude) { return false; }
    protected override bool GetTouch2DImpl(byte device, byte control, ref Vector2 position, ref byte state) { return false; }
    protected override bool IsActiveImpl(byte device, byte control) { return false; }
    protected override bool IsActiveImpl(byte device, byte control, AXRInputDirection direction) { return false; }
    protected override bool GetActivatedImpl(byte device, byte control) { return false; }
    protected override bool GetActivatedImpl(byte device, byte control, AXRInputDirection direction) { return false; }
    protected override bool GetDeactivatedImpl(byte device, byte control) { return false; }
    protected override bool GetDeactivatedImpl(byte device, byte control, AXRInputDirection direction) { return false; }

    protected override void UpdateInputFrameImpl() {
        AXRClientPlugin.UpdateInputFrame();
    }

    protected override void ClearInputImpl() {
        AXRClientPlugin.ClearInput();
    }
}
