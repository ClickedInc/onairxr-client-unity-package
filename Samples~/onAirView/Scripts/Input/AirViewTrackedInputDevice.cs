/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/
using onAirXR.Client;

public abstract class AirViewTrackedInputDevice : AXRInputSender {
    public bool useTrackingSpace;

    public void SetTrackingSpace(AirViewTrackingSpace trackingSpace) {
        this.trackingSpace = trackingSpace;
    }

    public void ClearTrackingSpace() {
        trackingSpace = null;
    }

    protected AirViewTrackingSpace trackingSpace { get; private set; }
}
