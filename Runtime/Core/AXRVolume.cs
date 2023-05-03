using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace onAirXR.Client {
    public class AXRVolume : MonoBehaviour {
        private Transform _thisTransform;
        private RenderEventEmitter _renderEventEmitter;
        private GameObject _tinyOpaqueObject;

        public Matrix4x4 worldToVolumeMatrix { get; private set; }

        public void Configure(Camera camera) {
            createTinyOpaqueObjectOntoFrustumEdge(camera);
        }

        public void ProcessPreRender(Camera camera) {
            if (_renderEventEmitter == null) {
                _renderEventEmitter = new RenderEventEmitter(camera, CameraEvent.AfterForwardOpaque);
            }
            _renderEventEmitter.OnPreRender(camera, _thisTransform);
        }

        public void ProcessPostRender(Camera camera) {
            _renderEventEmitter.OnPostRender();
        }

        private void Awake() {
            _thisTransform = transform;

            worldToVolumeMatrix = Matrix4x4.TRS(_thisTransform.position, _thisTransform.rotation, Vector3.one).inverse;
        }

        private void OnDestroy() {
            _renderEventEmitter?.Cleanup();

            if (_tinyOpaqueObject != null) {
                Destroy(_tinyOpaqueObject);
            }
        }

        private void createTinyOpaqueObjectOntoFrustumEdge(Camera camera) {
            // NOTE: ensure camera renders more than one opaque object to guarantee native mesh renderer to work
            var prefab = Resources.Load<GameObject>("AXRTinyOpaqueObject");
            if (prefab == null) { return; }

            if (_tinyOpaqueObject != null) {
                Destroy(_tinyOpaqueObject);
            }

            _tinyOpaqueObject = Instantiate(prefab, camera.transform);
            var frustum = camera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left).decomposeProjection;
            var y = frustum.bottom / frustum.zNear * frustum.zFar;
            _tinyOpaqueObject.transform.localPosition = new Vector3(0, y, -frustum.zFar);
            _tinyOpaqueObject.transform.localRotation = Quaternion.identity;
        }

        private class RenderEventEmitter {
            private AXRCameraEventRenderCommand _renderCommand;
            private RenderData[] _renderData = new RenderData[2];
            private IntPtr[] _nativeRenderData = new IntPtr[2];
            private int _eyeIndex = 0;

            public RenderEventEmitter(Camera camera, CameraEvent cameraEvent) {
                _renderCommand = new AXRCameraEventRenderCommand(camera, cameraEvent);
            }

            public void OnPreRender(Camera camera, Transform volumeTransform) {
                ensureNativeRenderDataAllocated(_eyeIndex);

                _renderData[_eyeIndex].Update(camera, _eyeIndex, volumeTransform);
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
            public static readonly int Size = sizeof(float) * 16 * 2;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public float[] anchorViewMatrix;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public float[] projectionMatrix;

            public void Update(Camera camera, int eyeIndex, Transform volumeTransform) {
                ensureMemoryAlocated();

                var eye = eyeIndex == 1 ? Camera.StereoscopicEye.Right : Camera.StereoscopicEye.Left;
                var anchorToView = camera.GetStereoViewMatrix(eye) * Matrix4x4.TRS(volumeTransform.position, volumeTransform.rotation, Vector3.one);
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
