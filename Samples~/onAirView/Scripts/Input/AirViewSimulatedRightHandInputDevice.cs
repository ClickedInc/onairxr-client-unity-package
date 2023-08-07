using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using onAirXR.Client;

public class AirViewSimulatedRightHandInputDevice : AXRTrackedInputDevice {
    private Camera _camera;
    private Transform _cameraTransform;
    private AirViewProfile _profile;

#if UNITY_EDITOR || UNITY_STANDALONE
    public AXRDeviceStatus currentStatus => AXRDeviceStatus.Ready;
#else
    private int activeTouchCount => Touchscreen.current.touches.Where((touch) => touch.press.ReadValue() > 0).Count();

    public AXRDeviceStatus currentStatus => activeTouchCount > 0 ? AXRDeviceStatus.Ready : AXRDeviceStatus.Unavailable;
#endif

    public Pose currentPose {
        get {
            if (currentStatus != AXRDeviceStatus.Ready) { return Pose.identity; }

#if UNITY_EDITOR || UNITY_STANDALONE
            var touchPos = Mouse.current.position;
#else
            var touchPos = getActiveTouch(0).position;
#endif
            var projection = _profile.GetCameraProjection();
            var scaledTouchPos = new Vector2(
                touchPos.x.ReadValue() / Display.main.renderingWidth,
                touchPos.y.ReadValue() / Display.main.renderingHeight
            );

            var cameraLocalPos = new Vector3((projection[0] * (1 - scaledTouchPos.x) + projection[2] * scaledTouchPos.x) * _camera.nearClipPlane,
                                             (projection[3] * (1 - scaledTouchPos.y) + projection[1] * scaledTouchPos.y) * _camera.nearClipPlane,
                                             _camera.nearClipPlane);
            var worldPos = _cameraTransform.localToWorldMatrix.MultiplyPoint(cameraLocalPos);

            var worldToAnchor = anchor?.worldToAnchorMatrix ?? Matrix4x4.identity;
            return new Pose(
                worldToAnchor.MultiplyPoint(worldPos),
                worldToAnchor.rotation * Quaternion.LookRotation(worldPos - _cameraTransform.position, Vector3.up)
            );
        }
    }

    public AirViewSimulatedRightHandInputDevice(Camera camera, IAXRAnchor anchor, AirViewProfile profile) : base(anchor) {
        _camera = camera;
        _cameraTransform = camera.transform;
        _profile = profile;
    }

    public override byte id => (byte)AXRInputDeviceID.RightHandTracker;

    public override void PendInputsPerFrame(AXRInputStream inputStream) {
        var pose = currentPose;
        
        inputStream.PendState(this, (byte)AXRHandTrackerControl.Status, (byte)currentStatus);
        inputStream.PendPose(this, (byte)AXRHandTrackerControl.Pose, pose.position, pose.rotation);
    }

    private TouchControl getActiveTouch(int index) {
        var activeTouches = Touchscreen.current.touches.Where((touch) => touch.press.ReadValue() > 0);
        if (index < 0 || index >= activeTouches.Count()) { return null; }

        return activeTouches.ToArray()[index];
    }
}
