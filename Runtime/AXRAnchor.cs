using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public class AXRAnchor {
        public enum Type {
            AppOrigin,
            GuardianRelative,
            AppRelative
        }

        public Type type => Type.AppOrigin;
        public Matrix4x4 worldToAnchorMatrix => Matrix4x4.identity;
        public Vector3 worldPosition => Vector3.zero;
        public Quaternion worldRotation => Quaternion.identity;
    }
}
