using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public abstract class AXRTrackedInputDevice : AXRInputSender {
        protected AXRAnchor anchor { get; private set; }

        public AXRTrackedInputDevice(AXRAnchor anchor) {
            this.anchor = anchor;
        }
    }
}
