using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    [RequireComponent(typeof(AudioSource))]
    public class AXRClientAudioRenderer : MonoBehaviour {
        private void OnAudioFilterRead(float[] data, int channels) {
            if (AXRClient.configured == false) { return; }

            if (AXRClientPlugin.GetAudioData(data, data.Length / channels, channels) == false) {
                // fill silence
                Array.Clear(data, 0, data.Length);
            }
        }
    }   
}
