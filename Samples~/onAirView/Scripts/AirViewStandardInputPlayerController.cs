using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirViewStandardInputPlayerController : MonoBehaviour {
    private Transform _thisTransform;
    private bool _lastMouseDown;
    private Vector3 _lastMousePos;

    private void Awake() {
        _thisTransform = transform;
    }

    private void Update() {
        rotateHead();
        moveHead();
    }

    private void rotateHead() {
        var mouseDown = Input.GetMouseButton(0);
        var mousePos = Input.mousePosition;

        if (mouseDown != _lastMouseDown) {
            _lastMouseDown = mouseDown;

            if (_lastMouseDown) {
                _lastMousePos = mousePos;
            }
        }

        if (mouseDown) {
            var scale = 1f / Mathf.Max(Display.main.renderingWidth, Display.main.renderingHeight);
            var delta = (mousePos - _lastMousePos) * scale;

            _thisTransform.Rotate(_thisTransform.right, delta.y * 360, Space.World);
            _thisTransform.Rotate(Vector3.up, -delta.x * 360, Space.World);

            _lastMousePos = mousePos;
        }
    }

    private void moveHead() {
        var forward = (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) + (Input.GetKey(KeyCode.DownArrow) ? -1 : 0);
        var right = (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) + (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0);

        const float Speed = 1.5f;
        _thisTransform.localPosition += (_thisTransform.forward * forward + _thisTransform.right * right).normalized * Speed * Time.deltaTime;
    }
}
