using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirViewStandardInputPlayerController : MonoBehaviour {
    private const float InitialHeight = 1.5f;

    private Transform _thisTransform;
    private bool _lastRotationEnabled;
    private Vector3 _lastMousePos;

    private void Awake() {
        _thisTransform = transform;
    }

    private void Start() {
        _thisTransform.localPosition = InitialHeight * Vector3.up;
    }

    private void Update() {
        rotateHead();
        moveHead();
    }

    private void rotateHead() {
        var rotationEnabled = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        var mousePos = Input.mousePosition;

        if (rotationEnabled != _lastRotationEnabled) {
            _lastRotationEnabled = rotationEnabled;

            if (_lastRotationEnabled) {
                _lastMousePos = mousePos;
            }
        }

        if (rotationEnabled) {
            var scale = 1f / Mathf.Max(Display.main.renderingWidth, Display.main.renderingHeight);
            var delta = (mousePos - _lastMousePos) * scale;

            _thisTransform.Rotate(_thisTransform.right, -delta.y * 360, Space.World);
            _thisTransform.Rotate(Vector3.up, delta.x * 360, Space.World);

            _lastMousePos = mousePos;
        }
    }

    private void moveHead() {
        var forward = (Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0);
        var right = (Input.GetKey(KeyCode.D) ? 1 : 0) + (Input.GetKey(KeyCode.A) ? -1 : 0);

        const float Speed = 1.5f;
        _thisTransform.localPosition += (_thisTransform.forward * forward + _thisTransform.right * right).normalized * Speed * Time.deltaTime;
    }
}
