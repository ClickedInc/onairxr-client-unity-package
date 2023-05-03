/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;

namespace onAirXR.Client {
    public abstract class AirVRTrackerInputDevice : AXRInputSender {
        private AXRVolume _volumeAnchor;

        public bool usingRealWorldSpace => realWorldSpace != null;

        public void setRealWorldSpace(AirVRRealWorldSpaceBase realWorldSpace) {
            this.realWorldSpace = realWorldSpace;
        }

        public void clearRealWorldSpace() {
            realWorldSpace = null;
        }

        public void SetVolumeAnchor(AXRVolume volume) {
            _volumeAnchor = volume;
        }

        public void ClearVolumeAnchor() {
            _volumeAnchor = null;
        }

        protected AirVRRealWorldSpaceBase realWorldSpace { get; private set; }
        protected bool isVolumeAnchorAvailable => _volumeAnchor != null;
        protected Matrix4x4 worldToVolumeMatrix => isVolumeAnchorAvailable ? _volumeAnchor.worldToVolumeMatrix : Matrix4x4.identity;
    }
}
