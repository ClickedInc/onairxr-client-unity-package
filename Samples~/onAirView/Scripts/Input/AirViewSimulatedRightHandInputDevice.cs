using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using onAirXR.Client;

public class AirViewSimulatedRightHandInputDevice : AXRTrackedInputDevice {
    private Camera _camera;
    private Transform _cameraTansform;

#if UNITY_EDITOR || UNITY_STANDALONE
    public AXRDeviceStatus currentStatus => AXRDeviceStatus.Ready;
#else
    public AXRDeviceStatus currentStatus => Touchscreen.current.touches.Count > 0 ? AXRDeviceStatus.Ready : AXRDeviceStatus.Unavailable;
#endif

    public Pose currentPose {
        get {
            if (currentStatus != AXRDeviceStatus.Ready) { return Pose.identity; }

#if UNITY_EDITOR || UNITY_STANDALONE
            var touchPos = Mouse.current.position;
#else
            var touchPos = Touchscreen.current.touches[0].position;
#endif
            var worldPos = _camera.ScreenToWorldPoint(new Vector3(touchPos.x.ReadValue(), touchPos.y.ReadValue(), _camera.nearClipPlane));

            var worldToAnchor = anchor?.worldToAnchorMatrix ?? Matrix4x4.identity;
            return new Pose(
                worldToAnchor.MultiplyPoint(worldPos),
                worldToAnchor.rotation * Quaternion.LookRotation(worldPos - _cameraTansform.position, Vector3.up)
            );
        }
    }

    public AirViewSimulatedRightHandInputDevice(Camera camera, IAXRAnchor anchor) : base(anchor) {
        _camera = camera;
        _cameraTansform = camera.transform;
    }

    public override byte id => (byte)AXRInputDeviceID.RightHandTracker;

    public override void PendInputsPerFrame(AXRInputStream inputStream) {
        var pose = currentPose;
        
        inputStream.PendState(this, (byte)AXRHandTrackerControl.Status, (byte)currentStatus);
        inputStream.PendPose(this, (byte)AXRHandTrackerControl.Pose, pose.position, pose.rotation);
    }
}
