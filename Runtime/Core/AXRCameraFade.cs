/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace onAirXR.Client {
    [RequireComponent(typeof(Camera))]
    public class AXRCameraFade : MonoBehaviour {
        private static Dictionary<string, List<AXRCameraFade>> _cameraFades = new Dictionary<string, List<AXRCameraFade>>();

        private static void addCameraFade(AXRCameraFade fade) {
            if (_cameraFades.ContainsKey(fade._tag) == false) {
                _cameraFades.Add(fade._tag, new List<AXRCameraFade>());
            }
            _cameraFades[fade._tag].Add(fade);
        }

        private static void removeCameraFade(AXRCameraFade fade) {
            if (_cameraFades.ContainsKey(fade._tag) == false) { return; }

            _cameraFades[fade._tag].Remove(fade);

            if (_cameraFades[fade._tag].Count == 0) {
                _cameraFades.Remove(fade._tag);
            }
        }

        public static async Task FadeAllCameras(string tag, int layer, Color from, Color to, float duration) {
            if (_cameraFades.ContainsKey(tag) == false) { return; }

            await Task.WhenAll(_cameraFades[tag].Select(fade => fade.fadeTask(layer, from, to, duration)));
        }

        public static void FadeAllCamerasImmediately(string tag, int layer, Color color) {
            if (_cameraFades.ContainsKey(tag) == false) { return; }

            foreach (var cameraFade in _cameraFades[tag]) {
                cameraFade.FadeImmediately(layer, color);
            }
        }

        private Transform _thisTransform;
        private Camera _camera;
        private CommandBuffer _commandBuffer;
        private bool _commandBufferApplied;
        private Mesh _fadeMesh;
        private Material _fadeMaterial;
        private Dictionary<int, Fader> _faders = new Dictionary<int, Fader>();

        [SerializeField] string _tag = "default";

        public bool isFading {
            get {
                foreach (var fader in _faders.Values) {
                    if (fader.fading) {
                        return true;
                    }
                }
                return false;
            }
        }

        public async void Fade(int layer, Color from, Color to, float duration) {
            await fadeTask(layer, from, to, duration);
        }

        public void FadeImmediately(int layer, Color color) {
            var shouldUpdateImmediately = isFading == false;

            _faders[layer] = new Fader(color);

            if (shouldUpdateImmediately) {
                _fadeMaterial.color = updateFadeColors(Time.realtimeSinceStartup);
            }
        }

        private void Awake() {
            _thisTransform = transform;
            _camera = GetComponent<Camera>();
            _commandBuffer = new CommandBuffer();

            prepareFadeMeshAndMaterial();

            addCameraFade(this);
        }

        private void OnDestroy() {
            if (_commandBufferApplied) {
                _camera.RemoveCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);
                _commandBufferApplied = false;
            }

            foreach (var fader in _faders.Values) {
                fader.Cancel();
            }
            _faders.Clear();

            removeCameraFade(this);
        }

        private void OnPreRender() {
            applyFadeCommands(_fadeMaterial.color != Color.clear);

            updateFadeCommands();
        }

        private void prepareFadeMeshAndMaterial() {
            const float FadeQuadAtan = 5f; // about 157 degrees of FOV

            _fadeMaterial = new Material(Shader.Find("onAirXR/Unlit transparent color"));
            _fadeMaterial.color = Color.clear;

            var distance = Mathf.Min((_camera.nearClipPlane + _camera.farClipPlane) / 2, _camera.nearClipPlane + 0.1f);
            var halfSize = distance * FadeQuadAtan;
            _fadeMesh = new Mesh();
            _fadeMesh.vertices = new Vector3[] {
            new Vector3(-halfSize, halfSize, distance),
            new Vector3(halfSize, halfSize, distance),
            new Vector3(-halfSize, -halfSize, distance),
            new Vector3(halfSize, -halfSize, distance)
        };
            _fadeMesh.uv = new Vector2[] {
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(0, 1)
        };
            _fadeMesh.triangles = new int[] { 0, 1, 2, 2, 1, 3 };
        }

        private void applyFadeCommands(bool apply) {
            if (apply == _commandBufferApplied) { return; }

            if (apply) {
                _camera.AddCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);
                _commandBufferApplied = true;
            }
            else {
                _camera.RemoveCommandBuffer(CameraEvent.AfterEverything, _commandBuffer);
                _commandBufferApplied = false;
            }
        }

        private void updateFadeCommands() {
            if (_commandBufferApplied == false) { return; }

            _commandBuffer.Clear();
            _commandBuffer.DrawMesh(_fadeMesh, _thisTransform.localToWorldMatrix, _fadeMaterial);
        }

        private async Task fadeTask(int layer, Color from, Color to, float duration) {
            if (isFading) {
                _faders[layer] = new Fader(from, to, duration);
                return;
            }

            _faders[layer] = new Fader(from, to, duration);

            do {
                _fadeMaterial.color = updateFadeColors(Time.realtimeSinceStartup);

                await Task.Yield();
            } while (isFading);
        }

        private Color updateFadeColors(float time) {
            var color = Color.clear;

            var layers = new List<int>(_faders.Keys);
            layers.Sort();

            foreach (var key in layers) {
                _faders[key].Update(time);

                var over = _faders[key].color;
                var col = over + color * (1 - over.a);
                var alpha = over.a + color.a * (1 - over.a);

                color = col;
                color.a = alpha;
            }
            return color;
        }

        private class Fader {
            private State _state = State.Ready;
            private Color _colorFrom;
            private Color _colorTo;
            private float _startTime;
            private float _duration;

            public Color color { get; private set; }
            public bool fading => _state != State.Done;

            public Fader(Color from, Color to, float duration) {
                _colorFrom = from;
                _colorTo = to;
                _duration = duration;
            }

            public Fader(Color color) {
                _colorFrom = _colorTo = color;
                _duration = 0;
            }

            public void Update(float realtime) {
                switch (_state) {
                    case State.Ready:
                        color = _colorFrom;
                        _startTime = realtime;
                        _state = _duration > 0 ? State.Fading : State.Done;
                        break;
                    case State.Fading:
                        color = Color.Lerp(_colorFrom, _colorTo, (realtime - _startTime) / _duration);

                        if (color == _colorTo) {
                            _state = State.Done;
                        }
                        break;
                }
            }

            public void Cancel() {
                _state = State.Done;
            }

            private enum State {
                Ready,
                Fading,
                Done
            }
        }
    }
}
