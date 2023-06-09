using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public abstract class AXRTrackedInputDevice : AXRInputSender {
        protected IAXRAnchor anchor { get; private set; }

        public AXRTrackedInputDevice(IAXRAnchor anchor) {
            this.anchor = anchor;
        }
    }
}
