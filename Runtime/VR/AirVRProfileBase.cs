﻿/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;
using UnityEngine;

namespace onAirXR.Client {
    [Serializable]
    public abstract class AirVRProfileBase {
        public const int ProfilerMaskFrame = 0x01;
        public const int ProfilerMaskReport = 0x02;

        public enum RenderType {
            DirectOnTwoEyeTextures,
            UseSeperateVideoRenderTarget
        }

        public enum VideoBitrate {
            Low,
            Normal,
            High,
            Best
        }

        [Serializable]
        public struct ProfilerSettings {
            public enum Flag : int {
                Frame = 0x01,
                Report = 0x02,
                Advanced = 0x04
            }

            public int Flags;
            public string Filename;
        }

        public AirVRProfileBase(VideoBitrate bitrate) {
            videoFrameRate = defaultVideoFrameRate;

            switch (bitrate) {
                case VideoBitrate.Low:
                    videoMinBitrate = 6000000;
                    videoStartBitrate = 8000000;
                    videoMaxBitrate = 16000000;
                    break;
                case VideoBitrate.Normal:
                    videoMinBitrate = 8000000;
                    videoStartBitrate = 16000000;
                    videoMaxBitrate = 28000000;
                    break;
                case VideoBitrate.High:
                    videoMinBitrate = 8000000;
                    videoStartBitrate = 24000000;
                    videoMaxBitrate = 40000000;
                    break;
                default:
                    break;
            }
        }

#pragma warning disable CS0414
        [SerializeField] private string UserID;
        [SerializeField] private ProfilerSettings Profiler;
        [SerializeField] private string[] SupportedVideoCodecs;
        [SerializeField] private string[] SupportedAudioCodecs;
        [SerializeField] private int VideoWidth;
        [SerializeField] private int VideoHeight;
        [SerializeField] private float VideoFrameRate;
        [SerializeField] private int VideoMinBitrate;
        [SerializeField] private int VideoStartBitrate;
        [SerializeField] private int VideoMaxBitrate;
        [SerializeField] private float IPD;
        [SerializeField] private bool Stereoscopy;
        [SerializeField] private float[] LeftEyeCameraNearPlane;
        [SerializeField] private Vector3 EyeCenterPosition;

        [SerializeField] private int[] LeftEyeViewport;
        [SerializeField] private int[] RightEyeViewport;
        [SerializeField] private float[] VideoScale;
#pragma warning restore CS0414

        private string[] supportedVideoCodecs => AXRClientPlugin.GetSupportedVideoCodecs();
        private string[] supportedAudioCodecs => AXRClientPlugin.GetSupportedAudioCodecs();

        private float[] leftEyeCameraNearPlaneScaled {
            get {
                float[] result = leftEyeCameraNearPlane;
                float[] scale = videoScale;
                result[0] *= scale[0];
                result[1] *= scale[1];
                result[2] *= scale[0];
                result[3] *= scale[1];

                return result;
            }
        }

        public abstract (int width, int height) defaultVideoResolution { get; }
        public abstract float defaultVideoFrameRate { get; }
        public abstract bool stereoscopy { get; }
        public abstract float[] leftEyeCameraNearPlane { get; }
        public abstract Vector3 eyeCenterPosition { get; }
        public abstract float ipd { get; }
        public abstract bool hasInput { get; }

        public abstract RenderType renderType { get; }
        public abstract int[] leftEyeViewport { get; }
        public abstract int[] rightEyeViewport { get; }
        public abstract float[] videoScale { get; }   // ratio of the size of the whole video rendered to the size of the area visible to an eye camera

        public abstract bool isUserPresent { get; }
        public abstract float delayToResumePlayback { get; }

        public virtual float[] videoRenderMeshVertices {
            get {
                return new float[] {
                -0.5f,  0.5f, 0.0f,
                0.5f,  0.5f, 0.0f,
                -0.5f, -0.5f, 0.0f,
                0.5f, -0.5f, 0.0f
            };
            }
        }

        public virtual float[] videoRenderMeshTexCoords {
            get {
                return new float[] {
                0.0f, 1.0f,
                1.0f, 1.0f,
                0.0f, 0.0f,
                1.0f, 0.0f
            };
            }
        }

        public virtual int[] videoRenderMeshIndices {
            get {
                return new int[] {
                0, 1, 2, 2, 1, 3
            };
            }
        }

        public bool useSeperateVideoRenderTarget {
            get {
                return renderType == RenderType.UseSeperateVideoRenderTarget;
            }
        }

        public bool useSingleTextureForEyes {
            get {
                return renderType == RenderType.UseSeperateVideoRenderTarget;
            }
        }

        public string userID {
            get {
                return UserID;
            }
            set {
                UserID = value;
            }
        }

        public (int width, int height) videoResolution {
            get {
                return (VideoWidth, VideoHeight);
            }
            set {
                VideoWidth = value.width;
                VideoHeight = value.height;
            }
        }

        public float videoFrameRate {
            get {
                return VideoFrameRate > 0 ? VideoFrameRate : defaultVideoFrameRate;
            }
            set {
                VideoFrameRate = value;
            }
        }

        public int videoMinBitrate {
            get {
                return VideoMinBitrate;
            }
            set {
                VideoMinBitrate = value;
            }
        }

        public int videoStartBitrate {
            get {
                return VideoStartBitrate;
            }
            set {
                VideoStartBitrate = value;
            }
        }

        public int videoMaxBitrate {
            get {
                return VideoMaxBitrate;
            }
            set {
                VideoMaxBitrate = value;
            }
        }

        public ProfilerSettings profiler {
            get {
                return Profiler;
            }
            set {
                Profiler = value;
            }
        }

        public AirVRProfileBase GetSerializable() {
            SupportedVideoCodecs = supportedVideoCodecs;
            SupportedAudioCodecs = supportedAudioCodecs;
            IPD = ipd;
            Stereoscopy = stereoscopy;
            LeftEyeCameraNearPlane = leftEyeCameraNearPlane;
            EyeCenterPosition = eyeCenterPosition;

            LeftEyeViewport = leftEyeViewport;
            RightEyeViewport = rightEyeViewport;
            VideoScale = videoScale;

            if (VideoWidth <= 0 || VideoHeight <= 0) {
                var res = defaultVideoResolution;
                VideoWidth = res.width;
                VideoHeight = res.height;
            }
            if (VideoFrameRate <= 0.0f) {
                VideoFrameRate = defaultVideoFrameRate;
            }
            return this;
        }

        public override string ToString() {
            var resolution = videoResolution;

            return string.Format("[AirVRProfile]\n" +
                                 "    videoWidth={0}\n" +
                                 "    videoHeight={1}\n" +
                                 "    videoFrameRate={2}\n" +
                                 "    videoScale=({3}, {4})\n" +
                                 "    render type={5}\n" +
                                 "    leftEyeViewport=({6}, {7}, {8}, {9})\n" +
                                 "    rightEyeViewport=({10}, {11}, {12}, {13})\n" +
                                 "    leftEyeCameraNearPlane=({14}, {15}, {16}, {17})\n" +
                                 "    eyeCenterPosition={18}\n" +
                                 "    ipd={19}\n" +
                                 "    stereoscopy={20}\n",
                                 resolution.width,
                                 resolution.height,
                                 videoFrameRate,
                                 videoScale[0], videoScale[1],
                                 renderType,
                                 leftEyeViewport[0], leftEyeViewport[1], leftEyeViewport[2], leftEyeViewport[3],
                                 rightEyeViewport[0], rightEyeViewport[1], rightEyeViewport[2], rightEyeViewport[3],
                                 leftEyeCameraNearPlane[0], leftEyeCameraNearPlane[1], leftEyeCameraNearPlane[2], leftEyeCameraNearPlane[3],
                                 eyeCenterPosition,
                                 ipd,
                                 stereoscopy);
        }
    }
}
