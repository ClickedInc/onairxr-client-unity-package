using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public class AXRRenderCamera : MonoBehaviour {
        private AXRCameraBase _owner;
        private Camera _thisCamera;

        public bool dedicated { get; private set; }
        public RenderTexture targetTexture {
            get { return _thisCamera.targetTexture; }
            set { _thisCamera.targetTexture = value; }
        }

        public void Configure(AXRCameraBase owner) {
            _owner = owner;
            dedicated = gameObject != _owner.gameObject;
            if (dedicated == false) { return; }

            _thisCamera.nearClipPlane = 0.01f;
            _thisCamera.farClipPlane = 0.02f;
            _thisCamera.depth = owner.depth - 1;
        }

        private void Awake() {
            _thisCamera = GetComponent<Camera>();
            if (_thisCamera == null) {
                _thisCamera = gameObject.AddComponent<Camera>();
            }
        }

        private void OnPreRender() {
            _owner?.PreRender(_thisCamera);
        }

        private void OnPostRender() {
            _owner?.PostRender(_thisCamera);
        }
    }
}
