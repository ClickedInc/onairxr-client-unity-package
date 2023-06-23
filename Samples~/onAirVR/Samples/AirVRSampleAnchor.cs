using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using onAirXR.Client;

public class AirVRSampleAnchor : AXRAnchorComponent {
    public override Matrix4x4 worldToAnchorMatrix => transform.worldToLocalMatrix;
}
