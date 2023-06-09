using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public class AXRHeadTrackerInputDevice : AXRTrackedInputDevice {
        private AXRCameraBase _camera;
        private Transform _head;

        public Pose currentPose {
            get {
                var worldToAnchor = anchor?.worldToAnchorMatrix ?? Matrix4x4.identity;
                return new Pose(
                    worldToAnchor.MultiplyPoint(_head.position),
                    worldToAnchor.rotation * _head.rotation
                );
            }
        }

        public AXRHeadTrackerInputDevice(AXRCameraBase camera, IAXRAnchor anchor, Transform head) : base(anchor) {
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
