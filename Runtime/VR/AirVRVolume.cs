using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    [RequireComponent(typeof(MeshRenderer))]
    public class AirVRVolume : MonoBehaviour {
        private Transform _thisTransform;
        private MeshRenderer _renderer;
        private Texture2D _decoderSurfaceTexture;

        public Matrix4x4 worldToVolumeMatrix { get; private set; }

        public void UpdateOnPreRender(Transform cameraTransform) {
            _thisTransform.rotation = cameraTransform.rotation;
        }

        private void Awake() {
            _thisTransform = transform;
            _renderer = GetComponent<MeshRenderer>();

            worldToVolumeMatrix = Matrix4x4.TRS(_thisTransform.position, _thisTransform.rotation, Vector3.one).inverse;
        }

        private void Start() {
            _renderer.enabled = false;
        }

        private void Update() {
            if (AXRClientPlugin.IsPlaying()) {
                retainDecoderSurface();
            }
            else {
                releaseDecorderSurface();
            }
        }

        private void retainDecoderSurface() {
            var nextTextureID = AXRClientPlugin.GetOffscreenFramebufferTexture();
            if (_decoderSurfaceTexture != null && _decoderSurfaceTexture.GetNativeTexturePtr().ToInt32() == nextTextureID) { return; }

            releaseDecorderSurface();
            if (nextTextureID == 0) { return; }

            _decoderSurfaceTexture = new Texture2D(3200, 1600, TextureFormat.ARGB32, false, true);
            _decoderSurfaceTexture.Apply();

            _decoderSurfaceTexture.UpdateExternalTexture(new IntPtr(nextTextureID));

            _renderer.material.mainTexture = _decoderSurfaceTexture;
            _renderer.enabled = true;
        }

        private void releaseDecorderSurface() {
            if (_decoderSurfaceTexture == null) { return; }

            _decoderSurfaceTexture = null;
            _renderer.material.mainTexture = null;
            _renderer.enabled = false;
        }
    }
}
