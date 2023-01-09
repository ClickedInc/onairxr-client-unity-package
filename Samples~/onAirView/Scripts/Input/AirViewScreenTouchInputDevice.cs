/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

public class AirViewTouchScreenInputDevice : AirViewTouchInputDevice {
    // implements AirViewTouchInputDevice
    public override byte id => (byte)AXRInputDeviceID.TouchScreen;

#if UNITY_EDITOR || UNITY_STANDALONE
    private TouchPhase _lastTouchPhase = TouchPhase.Stationary;

    protected override int touchCount => Input.GetMouseButton(0) || _lastTouchPhase == TouchPhase.Moved ? 1 : 0;
    protected override (int pointID, Vector2 position, TouchPhase phase) GetTouch(int index) {
        _lastTouchPhase = Input.GetMouseButtonDown(0) ?   TouchPhase.Began :
                          Input.GetMouseButtonUp(0) ?     TouchPhase.Ended :
                                                          TouchPhase.Moved;

        return (0,
                new Vector2(2 * Input.mousePosition.x / Screen.width - 1.0f, 
                            2 * Input.mousePosition.y / Screen.height - 1.0f),
                _lastTouchPhase
        );
    }
#else
    protected override int touchCount => Input.touchCount;
    protected override (int pointID, Vector2 position, TouchPhase phase) GetTouch(int index) {
        var touch = Input.GetTouch(index);
        return (touch.fingerId,
                new Vector2(2 * touch.position.x / Screen.width - 1.0f,
                            2 * touch.position.y / Screen.height - 1.0f),
                touch.phase);
    }
#endif
}
