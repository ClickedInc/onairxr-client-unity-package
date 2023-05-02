using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace onAirXR.Client {
    public class AXRVolume : MonoBehaviour {
        private Transform _thisTransform;

        private Mesh _mesh;
        private NativeMeshRenderer _nativeMeshRenderer;

        public Matrix4x4 worldToVolumeMatrix { get; private set; }

        public void Configure(Camera camera) {
            createTinyOpaqueObjectOntoFrustumEdge(camera);
        }

        public void ProcessPreRender(Camera camera) {
            if (_nativeMeshRenderer == null) {
                _nativeMeshRenderer = new NativeMeshRenderer(camera, CameraEvent.AfterForwardOpaque);
            }
            _nativeMeshRenderer.OnPreRender(_mesh, camera, _thisTransform, worldToVolumeMatrix);
        }

        public void ProcessPostRender(Camera camera) {
            _nativeMeshRenderer.OnPostRender();
        }

        private void Awake() {
            _thisTransform = transform;

            worldToVolumeMatrix = Matrix4x4.TRS(_thisTransform.position, _thisTransform.rotation, Vector3.one).inverse;
        }

        private void Start() {
            _mesh = createVolumeMesh();
        }

        private void OnDestroy() {
            _nativeMeshRenderer?.Cleanup();
        }

        private void createTinyOpaqueObjectOntoFrustumEdge(Camera camera) {
            // NOTE: ensure camera renders more than one opaque object to guarantee native mesh renderer to work
            var prefab = Resources.Load<GameObject>("AXRTinyOpaqueObject");
            if (prefab == null) { return; }

            var go = Instantiate(prefab, camera.transform);
            var frustum = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left).decomposeProjection;
            var y = frustum.bottom / frustum.zNear * frustum.zFar;
            go.transform.localPosition = new Vector3(0, y, -frustum.zFar);
            go.transform.localRotation = Quaternion.identity;
        }

        private Mesh createVolumeMesh() {
            var mesh = new Mesh();
            mesh.subMeshCount = 1;

            mesh.vertices = new[] {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f, -0.5f, -0.5f),
                new Vector3( 0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f,  0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f, -0.5f,  0.5f),
                new Vector3( 0.5f,  0.5f,  0.5f),
                new Vector3(-0.5f,  0.5f,  0.5f)
            };
            mesh.uv2 = new[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            mesh.SetIndices(new[] {
                2, 7, 3,
                2, 6, 7,
                4, 5, 1,
                4, 1, 0,
                7, 4, 0,
                7, 0, 3,
                6, 2, 1,
                6, 1, 5,
                7, 6, 5,
                7, 5, 4,
                3, 0, 1,
                3, 1, 2
            }, MeshTopology.Triangles, 0);

            mesh.SetVertexBufferParams(
                mesh.vertices.Length,
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, 1)
            );

            return mesh;
        }

        private class NativeMeshRenderer {
            private AXRCameraEventRenderCommand _renderCommand;
            private RenderData[] _renderData = new RenderData[2];
            private IntPtr[] _nativeRenderData = new IntPtr[2];
            private int _eyeIndex = 0;

            public NativeMeshRenderer(Camera camera, CameraEvent cameraEvent) {
                _renderCommand = new AXRCameraEventRenderCommand(camera, cameraEvent);
            }

            public void OnPreRender(Mesh mesh, Camera camera, Transform modelTransform, Matrix4x4 worldToVolumeMatrix) {
                ensureNativeRenderDataAllocated(_eyeIndex);

                _renderData[_eyeIndex].Update(mesh, camera, _eyeIndex, modelTransform, worldToVolumeMatrix);
                Marshal.StructureToPtr(_renderData[_eyeIndex], _nativeRenderData[_eyeIndex], true);

                AXRClientPlugin.RenderVolume(_renderCommand,
                                             _eyeIndex == 1 ? AXRClientPlugin.FrameType.StereoRight : AXRClientPlugin.FrameType.StereoLeft,
                                             _nativeRenderData[_eyeIndex]);

                _eyeIndex = 1 - _eyeIndex;
            }

            public void OnPostRender() {
                _renderCommand.Clear();
            }

            public void Cleanup() {
                for (var index = 0; index < 2; index++) {
                    if (_nativeRenderData[index] == IntPtr.Zero) { continue; }

                    Marshal.FreeHGlobal(_nativeRenderData[index]);
                    _nativeRenderData[index] = IntPtr.Zero;
                }
            }

            private void ensureNativeRenderDataAllocated(int eyeIndex) {
                if (_nativeRenderData[eyeIndex] != IntPtr.Zero) { return; }

                _nativeRenderData[eyeIndex] = Marshal.AllocHGlobal(RenderData.Size);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RenderData {
            public static readonly int Size = IntPtr.Size * 3 + sizeof(int) + sizeof(float) * 16 * 2;

            public IntPtr vertices;
            public IntPtr texcoords;
            public IntPtr indices;
            public int indexCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public float[] anchorViewMatrix;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public float[] projectionMatrix;

            public void Update(Mesh mesh, Camera camera, int eyeIndex, Transform modelTransform, Matrix4x4 worldToVolume) {
                ensureMemoryAlocated();

                vertices = mesh.GetNativeVertexBufferPtr(0);
                texcoords = mesh.GetNativeVertexBufferPtr(1);
                indices = mesh.GetNativeIndexBufferPtr();
                indexCount = (int)mesh.GetIndexCount(0);

                var eye = eyeIndex == 1 ? Camera.StereoscopicEye.Right : Camera.StereoscopicEye.Left;
                var anchorToView = camera.GetStereoViewMatrix(eye) * Matrix4x4.TRS(modelTransform.position, modelTransform.rotation, Vector3.one);
                var projection = GL.GetGPUProjectionMatrix(camera.GetStereoProjectionMatrix(eye), true);
                
                writeMatrixToFloatArray(anchorToView, ref anchorViewMatrix);
                writeMatrixToFloatArray(projection, ref projectionMatrix);
            }

            private void ensureMemoryAlocated() {
                if (anchorViewMatrix == null) {
                    anchorViewMatrix = new float[16];
                }
                if (projectionMatrix == null) {
                    projectionMatrix = new float[16];
                }
            }

            private void writeMatrixToFloatArray(Matrix4x4 mat, ref float[] dest) {
                dest[0] = mat.m00;
                dest[1] = mat.m10;
                dest[2] = mat.m20;
                dest[3] = mat.m30;

                dest[4] = mat.m01;
                dest[5] = mat.m11;
                dest[6] = mat.m21;
                dest[7] = mat.m31;

                dest[8] = mat.m02;
                dest[9] = mat.m12;
                dest[10] = mat.m22;
                dest[11] = mat.m32;

                dest[12] = mat.m03;
                dest[13] = mat.m13;
                dest[14] = mat.m23;
                dest[15] = mat.m33;
            }
        }
    }
}
