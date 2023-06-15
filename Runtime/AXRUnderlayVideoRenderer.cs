using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public class AXRUnderlayVideoRenderer {
        private Transform _cameraTransform;
        private Camera _camera;
        private MeshFilter _meshFilter;
        private MeshRenderer _renderer;

        public RenderTexture texture { get; private set; }

        public bool enabled {
            get { return _renderer.enabled; }
            set { _renderer.enabled = value; }
        }

        public AXRUnderlayVideoRenderer(GameObject owner, AXRProfileBase profile, AXRCameraBase camera) {
            _camera = camera.GetComponent<Camera>();
            _cameraTransform = camera.transform;

            const float Depth = 0.9f;

            var mesh = new Mesh();
            mesh.vertices = new Vector3[] {
                new Vector3(-1,  1, Depth),
                new Vector3( 1,  1, Depth),
                new Vector3(-1, -1, Depth),
                new Vector3( 1, -1, Depth)
            };
            mesh.uv = new Vector2[] {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f)
            };
            mesh.triangles = new int[] {
                0, 1, 2, 2, 1, 3
            };

            _meshFilter = owner.GetComponent<MeshFilter>();
            if (_meshFilter == null) {
                _meshFilter = owner.AddComponent<MeshFilter>();
            }
            _meshFilter.mesh = mesh;
            _meshFilter.mesh.UploadMeshData(true);

            _renderer = owner.GetComponent<MeshRenderer>();
            if (_renderer == null) {
                _renderer = owner.AddComponent<MeshRenderer>();
            }
            _renderer.material = new Material(Shader.Find("onAirXR/Video frame on far clip plane"));
            _renderer.enabled = false;
        }

        public void Start(AXRProfileBase profile) {
            if (texture != null) { return; }

            texture = new RenderTexture(profile.videoResolution.width, profile.videoResolution.height, 0, RenderTextureFormat.ARGB32);
            texture.useMipMap = false;
            texture.autoGenerateMips = false;
            texture.filterMode = FilterMode.Bilinear;
            texture.anisoLevel = 0;
            texture.Create();

            _renderer.material.mainTexture = texture;
            enabled = true;
        }

        public void Stop() {
            if (texture == null) { return; }

            enabled = false;
            _renderer.material.mainTexture = null;

            texture.Release();
            texture = null;
        }

        public void LateUpdate() {
            _meshFilter.mesh.bounds = new Bounds(_cameraTransform.position + 
                                                 _cameraTransform.forward * (_camera.nearClipPlane + _camera.farClipPlane) / 2, 
                                                 Vector2.one);
        }
    }
}
