/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

public class AirViewMotionTrackerInputDevice : AirViewTrackedInputDevice {
    public Transform tracker { get; set; }

    public Pose currentPose {
        get {
            if (trackingSpace != null) {
                var worldToTrackingSpaceMatrix = trackingSpace.trackingSpaceToWorldMatrix.inverse;

                return new Pose(
                    worldToTrackingSpaceMatrix.MultiplyPoint(tracker.position),
                    worldToTrackingSpaceMatrix.rotation * tracker.rotation
                );
            }
            else {
                return new Pose(
                    tracker.localPosition,
                    tracker.localRotation
                );
            }
        }
    }

    // implements AirViewTrackedInputDevice
    public override byte id => (byte)AXRInputDeviceID.HeadTracker;

    public override void PendInputsPerFrame(AXRInputStream inputStream) {
        if (tracker) {
            var pose = currentPose;
            inputStream.PendPose(this, (byte)AXRHeadTrackerControl.Pose, pose.position, pose.rotation);
        }
        inputStream.PendByteAxis(this, (byte)AXRHeadTrackerControl.Battery, (byte)Mathf.Round(SystemInfo.batteryLevel * 100));
    }
}
