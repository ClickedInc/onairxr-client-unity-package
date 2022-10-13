﻿/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

public class AirVROVRInputHelper {
    public enum HeadsetType {
        Unknown,
        GearVR,
        Go,
        Quest
    }

    public static HeadsetType GetHeadsetType() {
        switch (OVRPlugin.GetSystemHeadsetType()) {
            case OVRPlugin.SystemHeadset.Oculus_Quest:
            case OVRPlugin.SystemHeadset.Oculus_Quest_2:
                return HeadsetType.Quest;
            default:
                return HeadsetType.Unknown;
        }
    }

    public static bool IsConnected(OVRInput.Controller controller) => OVRInput.IsControllerConnected(controller) && OVRInput.GetControllerPositionTracked(controller);
}
