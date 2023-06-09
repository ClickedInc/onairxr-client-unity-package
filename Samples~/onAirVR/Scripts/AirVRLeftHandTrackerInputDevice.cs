/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

public class AirVRLeftHandTrackerInputDevice : AXRTrackedInputDevice {
    private Transform _trackingSpace;

    public AXRDeviceStatus currentStatus => AirVROVRInputHelper.IsConnected(OVRInput.Controller.LTouch) ? AXRDeviceStatus.Ready : AXRDeviceStatus.Unavailable;

    public Pose currentPose {
        get {
            const OVRInput.Controller controller = OVRInput.Controller.LTouch;

            var trackingSpaceToWorld = _trackingSpace.localToWorldMatrix;
            var pos = trackingSpaceToWorld.MultiplyPoint(OVRInput.GetLocalControllerPosition(controller));
            var rot = trackingSpaceToWorld.rotation *  OVRInput.GetLocalControllerRotation(controller);

            var worldToAnchor = anchor?.worldToAnchorMatrix ?? Matrix4x4.identity;
            return new Pose(
                worldToAnchor.MultiplyPoint(pos),
                worldToAnchor.rotation * rot
            );
        }
    }

    public AirVRLeftHandTrackerInputDevice(Transform trackingSpace, IAXRAnchor anchor) : base(anchor) {
        _trackingSpace = trackingSpace;
    }

    public override byte id => (byte)AXRInputDeviceID.LeftHandTracker;

    public override void PendInputsPerFrame(AXRInputStream inputStream) {
        var pose = currentPose;

        inputStream.PendState(this, (byte)AXRHandTrackerControl.Status, (byte)currentStatus);
        inputStream.PendPose(this, (byte)AXRHandTrackerControl.Pose, pose.position, pose.rotation);

        var battery = OVRInput.GetControllerBatteryPercentRemaining(OVRInput.Controller.LTouch);
        inputStream.PendByteAxis(this, (byte)AXRHandTrackerControl.Battery, battery > 0 ? battery : (byte)255);
    }
}
