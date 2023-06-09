using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public abstract class AXRInputDeviceFeedbackBase : MonoBehaviour {
        private AXRTicker _vibrationTicker = new AXRTicker();

        protected abstract AXRInputDeviceID srcDevice { get; }
        protected abstract bool srcDeviceConnected { get; }

        protected abstract void SetVibration(float frequency, float amplitude);

        public void OnPreLink(AXRProfileBase profile) {
            _vibrationTicker.Set(profile.videoFrameRate);
        }

        private void LateUpdate() {
            updateVibration();
        }

        private void updateVibration() {
            _vibrationTicker.UpdatePerFrame();
            if (_vibrationTicker.tickOnFrame == false) { return; }

            float frequency = 0.0f, amplitude = 0.0f;
            if (AXRClient.inputManager?.inputStream?.GetVibrationFrame((byte)srcDevice, (byte)AXRHandTrackerFeedbackControl.Vibration, ref frequency, ref amplitude) ?? false) {
                SetVibration(frequency, amplitude);
            }
        }
    }
}
