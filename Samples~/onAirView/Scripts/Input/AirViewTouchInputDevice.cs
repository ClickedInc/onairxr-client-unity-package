/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using onAirXR.Client;

public abstract class AirViewTouchInputDevice : AXRInputSender {
    private const int MaxTouchCount = 10;
    private const int InvalidPointID = int.MaxValue;

    private int[] _mapTouchIDs = new int[MaxTouchCount];

    public AirViewTouchInputDevice() {
        foreach (var index in Enumerable.Range(0, MaxTouchCount)) {
            _mapTouchIDs[index] = InvalidPointID;
        }
    }

    protected abstract int touchCount { get; }
    protected abstract (int pointID, Vector2 position, TouchPhase phase) GetTouch(int index);

    // implements AXRInputSender
    public override void PendInputsPerFrame(AXRInputStream inputStream) {
        if (touchCount == 0) { return; }

        var count = Mathf.Min(touchCount, MaxTouchCount);
        foreach (var index in Enumerable.Range(0, count)) {
            try {
                var touch = GetTouch(index);
                var (touchID, phase) = retainTouch(touch.pointID, touch.phase);
                var active = phase != AXRTouchPhase.Ended && phase != AXRTouchPhase.Canceled;

                inputStream.PendTouch2D(id, touchID, touch.position, (byte)phase, active);

                if (phase == AXRTouchPhase.Ended ||
                    phase == AXRTouchPhase.Canceled) {
                    releaseTouch(touchID);
                }
            }
            catch (UnityException) { }
        }
    }

    private (byte id, AXRTouchPhase phase) retainTouch(int pointID, TouchPhase phaseInput) {
        var touchID = retainTouchID(pointID);
        if (touchID < 0) { throw new UnityException("invalid touch ID"); }

        var phase = AXRTouchPhase.Ended;
        switch (phaseInput) {
            case TouchPhase.Began:
            case TouchPhase.Stationary:
                phase = AXRTouchPhase.Stationary;
                break;
            case TouchPhase.Moved:
                phase = AXRTouchPhase.Moved;
                break;
            case TouchPhase.Canceled:
                phase = AXRTouchPhase.Canceled;
                break;
        }

        return ((byte)touchID, phase);
    }

    private int retainTouchID(int fingerID) {
        var nextTouchID = -1;

        foreach (var touchID in Enumerable.Range(0, MaxTouchCount)) {
            if (_mapTouchIDs[touchID] == fingerID) {
                return touchID;
            }
            else if (nextTouchID < 0 && _mapTouchIDs[touchID] == InvalidPointID) {
                nextTouchID = touchID;
            }
        }
        if (nextTouchID >= 0) {
            _mapTouchIDs[nextTouchID] = fingerID;
        }
        return nextTouchID;
    }

    private void releaseTouch(int id) {
        Assert.IsTrue(0 <= id && id < MaxTouchCount);

        _mapTouchIDs[id] = InvalidPointID;
    }
}
