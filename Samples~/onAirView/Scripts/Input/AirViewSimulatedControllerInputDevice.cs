using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using onAirXR.Client;

public class AirViewSimulatedControllerInputDevice : AXRInputSender {
    public override byte id => (byte)AXRInputDeviceID.Controller;

    public override void PendInputsPerFrame(AXRInputStream inputStream) {
#if UNITY_EDITOR || UNITY_STANDALONE
        var rIndexTrigger = Mouse.current.leftButton.isPressed;
        var rHandTrigger = Mouse.current.rightButton.isPressed;

        inputStream.PendAxis(this, (byte)AXRControllerControl.AxisRIndexTrigger, rIndexTrigger ? 1.0f : 0.0f);
        inputStream.PendAxis(this, (byte)AXRControllerControl.AxisRHandTrigger, rHandTrigger ? 1.0f : 0.0f);
#else
        var rIndexTrigger = Touchscreen.current.touches.Count > 0 ? Touchscreen.current.touches[0].isPressed : false;
        var lIndexTrigger = Touchscreen.current.touches.Count > 1 ? Touchscreen.current.touches[1].isPressed : false;

        inputStream.PendAxis(this, (byte)AXRControllerControl.AxisRIndexTrigger, rIndexTrigger ? 1.0f : 0.0f);
        inputStream.PendAxis(this, (byte)AXRControllerControl.AxisLIndexTrigger, lIndexTrigger ? 1.0f : 0.0f);
#endif
    }
}
