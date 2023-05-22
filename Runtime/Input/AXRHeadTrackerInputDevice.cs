using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public class AXRHeadTrackerInputDevice : AXRTrackedInputDevice {
        private AXRCameraBase _camera;
        private Transform _head;

        public Pose currentPose => new Pose(
            anchor.worldToAnchorMatrix.MultiplyPoint(_head.position),
            anchor.worldToAnchorMatrix.rotation * _head.rotation
        );

        public AXRHeadTrackerInputDevice(AXRCameraBase camera, AXRAnchor anchor, Transform head) : base(anchor) {
            _camera = camera;
            _head = head;
        }

        // implements AXRTrackedInputDevice
        public override byte id => (byte)AXRInputDeviceID.HeadTracker;

        public override void PendInputsPerFrame(AXRInputStream inputStream) {
            var pose = currentPose;

            inputStream.PendPose(this, (byte)AXRHeadTrackerControl.Pose, pose.position, pose.rotation);
            inputStream.PendByteAxis(this, (byte)AXRHeadTrackerControl.Battery, (byte)Mathf.Round(_camera.deviceBatteryLevel * 100));
        }
    }
}
