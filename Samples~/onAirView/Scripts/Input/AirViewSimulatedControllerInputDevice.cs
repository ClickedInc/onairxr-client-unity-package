using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using onAirXR.Client;

public class AirViewSimulatedControllerInputDevice : AXRInputSender {
    private const int DelayFramesToPress = 2;

    private int _remainingFramesToRIndexTrigger;
    private int _remainingFramesToLIndexTrigger;

    public override byte id => (byte)AXRInputDeviceID.Controller;

    public override void PendInputsPerFrame(AXRInputStream inputStream) {
#if UNITY_EDITOR || UNITY_STANDALONE
        var rIndexTrigger = Mouse.current.leftButton.isPressed;
        var rHandTrigger = Mouse.current.rightButton.isPressed;

        inputStream.PendAxis(this, (byte)AXRControllerControl.AxisRIndexTrigger, rIndexTrigger ? 1.0f : 0.0f);
        inputStream.PendAxis(this, (byte)AXRControllerControl.AxisRHandTrigger, rHandTrigger ? 1.0f : 0.0f);
#else
        var activeTouches = Touchscreen.current.touches.Where((touch) => touch.press.ReadValue() > 0).ToArray();

        var primaryPress = activeTouches.Length > 0 ? activeTouches[0].press.ReadValue() > 0 : false;
        var secondaryPress = activeTouches.Length > 1 ? activeTouches[1].press.ReadValue() > 0 : false;

        if (primaryPress) {
            _remainingFramesToRIndexTrigger = _remainingFramesToRIndexTrigger >= 0 ? _remainingFramesToRIndexTrigger - 1 : -1;
        }
        else {
            _remainingFramesToRIndexTrigger = DelayFramesToPress;
        }

        if (secondaryPress) {
            _remainingFramesToLIndexTrigger = _remainingFramesToLIndexTrigger >= 0 ? _remainingFramesToLIndexTrigger - 1 : -1;
        }
        else {
            _remainingFramesToLIndexTrigger = DelayFramesToPress;
        }

        inputStream.PendAxis(this, (byte)AXRControllerControl.AxisRIndexTrigger, _remainingFramesToRIndexTrigger < 0 ? 1.0f : 0.0f);
        inputStream.PendAxis(this, (byte)AXRControllerControl.AxisLIndexTrigger, _remainingFramesToLIndexTrigger < 0 ? 1.0f : 0.0f);
#endif
    }
}
