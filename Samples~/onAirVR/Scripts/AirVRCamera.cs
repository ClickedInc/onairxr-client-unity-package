/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

[RequireComponent(typeof(Camera))]
public class AirVRCamera : AXRCameraBase {
    private AirVRProfile _profile;
    private AirVRControllerInputDevice _controller;
    private AirVRLeftHandTrackerFeedback _leftHandTrackerFeedback;
    private AirVRRightHandTrackerFeedback _rightHandTrackerFeedback;

    [SerializeField] private AXRAnchorComponent _anchor = null;

    public AirVRLeftHandTrackerInputDevice leftHandTracker { get; private set; }
    public AirVRRightHandTrackerInputDevice rightHandTracker { get; private set; }

    protected override IAXRAnchor anchor => _anchor;
    public override AXRProfileBase profile => _profile;
    public override float deviceBatteryLevel => SystemInfo.batteryLevel;

    public override void OnPreLink() {
        _leftHandTrackerFeedback.OnPreLink(_profile);
        _rightHandTrackerFeedback.OnPreLink(_profile);
    }

    protected override void Awake() {
        _profile = new AirVRProfile();

        base.Awake();
    }

    protected override void Start() {
        base.Start();

        _leftHandTrackerFeedback = gameObject.AddComponent<AirVRLeftHandTrackerFeedback>();
        _rightHandTrackerFeedback = gameObject.AddComponent<AirVRRightHandTrackerFeedback>();
    }

    protected override void OnRecenterPose() {
        // NOTE: recenter pose may not work on recent Oculus runtimes
        OVRManager.display.RecenterPose();
    }

    protected override void OnPostStart(IAXRAnchor anchor) {
        var trackingSpace = transform.parent;
        leftHandTracker = new AirVRLeftHandTrackerInputDevice(trackingSpace, anchor);
        rightHandTracker = new AirVRRightHandTrackerInputDevice(trackingSpace, anchor);
        _controller = new AirVRControllerInputDevice();

        AXRClient.inputManager.Register(leftHandTracker);
        AXRClient.inputManager.Register(rightHandTracker);
        AXRClient.inputManager.Register(_controller);
    }
}
