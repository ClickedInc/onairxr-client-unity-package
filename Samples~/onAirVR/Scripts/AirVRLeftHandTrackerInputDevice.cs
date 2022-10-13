﻿/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

public class AirVRLeftHandTrackerInputDevice : AirVRTrackerInputDevice {
    public AXRDeviceStatus currentStatus {
        get {
            return  AirVROVRInputHelper.IsConnected(OVRInput.Controller.LTouch) ? AXRDeviceStatus.Ready : AXRDeviceStatus.Unavailable;
        }
    }

    public Pose currentPose {
        get {
            const OVRInput.Controller controller = OVRInput.Controller.LTouch;

            var position = OVRInput.GetLocalControllerPosition(controller);
            var rotation = OVRInput.GetLocalControllerRotation(controller);

            if (realWorldSpace != null) {
                var trackingSpaceToRealWorldMatrix = (realWorldSpace as AirVRRealWorldSpace).trackingSpaceToRealWorldMatrix;

                return new Pose(
                    trackingSpaceToRealWorldMatrix.MultiplyPoint(position),
                    trackingSpaceToRealWorldMatrix.rotation * rotation
                );
            }
            else {
                return new Pose(position, rotation);
            }
        }
    }

    // implements AirVRTrackerInputDevice
    public override byte id => (byte)AXRInputDeviceID.LeftHandTracker;

    public override void PendInputsPerFrame(AXRInputStream inputStream) {
        var pose = currentPose;

        inputStream.PendState(this, (byte)AXRHandTrackerControl.Status, (byte)currentStatus);
        inputStream.PendPose(this, (byte)AXRHandTrackerControl.Pose, pose.position, pose.rotation);

        var battery = OVRInput.GetControllerBatteryPercentRemaining(OVRInput.Controller.LTouch);
        inputStream.PendByteAxis(this, (byte)AXRHandTrackerControl.Battery, battery > 0 ? battery : (byte)255);
    }
}
