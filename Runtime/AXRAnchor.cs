using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public interface IAXRAnchor {
        Matrix4x4 worldToAnchorMatrix { get; }
    }

    public abstract class AXRAnchorComponent : MonoBehaviour, IAXRAnchor {
        public abstract Matrix4x4 worldToAnchorMatrix { get; }
    }
}
