/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

namespace onAirXR.Client {
    public abstract class AirVRTrackerInputDevice : AXRInputSender {
        public bool usingRealWorldSpace => realWorldSpace != null;

        public void setRealWorldSpace(AirVRRealWorldSpaceBase realWorldSpace) {
            this.realWorldSpace = realWorldSpace;
        }

        public void clearRealWorldSpace() {
            realWorldSpace = null;
        }

        public void SetReferenceVolume(AXRVolume volume) {
            referenceVolume = volume;
        }

        public void ClearReferenceVolume() {
            referenceVolume = null;
        }

        protected AirVRRealWorldSpaceBase realWorldSpace { get; private set; }
        protected AXRVolume referenceVolume { get; private set; }
    }
}
