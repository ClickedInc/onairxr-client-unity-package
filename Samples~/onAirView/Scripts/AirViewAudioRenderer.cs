/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;
using System.Threading.Tasks;
using UnityEngine;
using onAirXR.Client;

[RequireComponent(typeof(AudioSource))]
public class AirViewAudioRenderer : MonoBehaviour {
    private void OnAudioFilterRead(float[] data, int channels) {
        if (AirViewClient.configured == false) { return; }

        if (AXRClientPlugin.GetAudioData(data, data.Length / channels, channels) == false) {
            Array.Clear(data, 0, data.Length);
        }
    }
}
