/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;

public class AirViewTrackingSpace {
    private AirViewCamera _camera;
    private Vector3 _originInWorldSpace = Vector3.zero;
    private Vector3 _frontInWorldSpace = Vector3.forward;

    public Matrix4x4 trackingSpaceToWorldMatrix { get; private set; }

    public AirViewTrackingSpace(AirViewCamera camera) {
        _camera = camera;
    }

    public void SetOrigin(Vector3 worldPosition) {
        var offset = _frontInWorldSpace - _originInWorldSpace;

        _originInWorldSpace = worldPosition;
        _frontInWorldSpace = _originInWorldSpace + offset;
    }

    public void SetFront(Vector3 worldPosition) {
        _frontInWorldSpace = worldPosition;
    }

    public void Update() {
        trackingSpaceToWorldMatrix = calcTrackingSpaceToWorldMatrix();
    }

    private Matrix4x4 calcTrackingSpaceToWorldMatrix() {
        return Matrix4x4.TRS(
            _originInWorldSpace,
            Quaternion.LookRotation(_frontInWorldSpace - _originInWorldSpace, Vector3.up),
            Vector3.one
        );
    }
}
