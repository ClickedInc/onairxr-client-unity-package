/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

[RequireComponent(typeof(Camera))]

public class AirVRCamera : AirVRCameraBase {
    private static AirVRCamera _instance;

    [SerializeField] private bool _preferRealWorldSpace = false;

    private Transform _trackingSpace;
    private AirVRProfile _profile;
    private AirVRLeftHandTracker _leftHandTrackerFeedback;
    private AirVRRightHandTracker _rightHandTrackerFeedback;

    public AirVRRealWorldSpace realWorldSpace { get; private set; }
    public override Matrix4x4 trackingSpaceToWorldMatrix => _trackingSpace.localToWorldMatrix;

    public AirVRLeftHandTrackerInputDevice leftHandTracker { get; private set; }
    public AirVRRightHandTrackerInputDevice rightHandTracker { get; private set; }

    public override void OnPreConnect() {
        _leftHandTrackerFeedback?.OnPreConnect(_profile);
        _rightHandTrackerFeedback?.OnPreConnect(_profile);
    }

    protected override void Awake () {
        if (Application.isEditor) {
            AirVRClient.automaticallyPauseWhenUserNotPresent = false;
        }

        AXRClientPlugin.Load();
        
        base.Awake();
        _profile = new AirVRProfile(videoBitrate);
        _trackingSpace = transform.parent;
    }

    protected override void Start() {
        base.Start();

        leftHandTracker = new AirVRLeftHandTrackerInputDevice();
        rightHandTracker = new AirVRRightHandTrackerInputDevice();

        AirVRInputManager.RegisterInputSender(leftHandTracker);
        AirVRInputManager.RegisterInputSender(rightHandTracker);
        AirVRInputManager.RegisterInputSender(new AirVRControllerInputDevice());

        var desc = pointerDesc;
        _leftHandTrackerFeedback = gameObject.AddComponent<AirVRLeftHandTracker>();
        _leftHandTrackerFeedback.Configure(_profile, leftControllerModel, desc);

        _rightHandTrackerFeedback = gameObject.AddComponent<AirVRRightHandTracker>();
        _rightHandTrackerFeedback.Configure(_profile, rightControllerModel, desc);

        if (_preferRealWorldSpace && 
            (Application.isEditor || AirVROVRInputHelper.GetHeadsetType() == AirVROVRInputHelper.HeadsetType.Quest)) {
            realWorldSpace = new AirVRRealWorldSpace(this);

            headTracker.setRealWorldSpace(realWorldSpace);
            leftHandTracker.setRealWorldSpace(realWorldSpace);
            rightHandTracker.setRealWorldSpace(realWorldSpace);
        }
    }

    private void Update() {
        if (realWorldSpace != null) {
            realWorldSpace.Update();
        }
    }

    public override AirVRProfileBase profile => _profile;

    protected override void RecenterPose() {
        OVRManager.display.RecenterPose();
    }
}
