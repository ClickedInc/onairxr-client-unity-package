/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;
using System.IO;
using UnityEngine;

namespace onAirXR.Client {
    #pragma warning disable 0649

    [Serializable]
    public class AXRClientConfig {
        [Serializable]
        public struct VideoBitrate {
            public int min;
            public int start;
            public int max;
        }

        [Serializable]
        public struct VideoResolution {
            public int width;
            public int height;
        }

        public string address;
        public int port;
        public string userID;
        public VideoBitrate videoBitrate;
        public VideoResolution videoResolution;
        public float videoFramerate;
    }

    #pragma warning restore 0649
}