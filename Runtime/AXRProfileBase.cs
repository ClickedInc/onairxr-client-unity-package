using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    [Serializable]
    public abstract class AXRProfileBase {
        public enum RenderType {
            DirectOnFrameBufferTexture,
            VideoRenderTextureInScene
        }

        #pragma warning disable CS0414
        [SerializeField] private string UserID;
        [SerializeField] private string Place;
        [SerializeField] private string TempPath;
        [SerializeField] private string[] SupportedVideoCodecs;
        [SerializeField] private string[] SupportedAudioCodecs;
        [SerializeField] private int VideoWidth;
        [SerializeField] private int VideoHeight;
        [SerializeField] private float VideoFrameRate;
        [SerializeField] private int VideoMinBitrate;
        [SerializeField] private int VideoStartBitrate;
        [SerializeField] private int VideoMaxBitrate;
        [SerializeField] private bool Stereoscopy;
        [SerializeField] private float[] VideoScale;

        // for stereosopic
        [SerializeField] private float[] LeftEyeCameraNearPlane;        
        [SerializeField] private float IPD;
        [SerializeField] private int[] LeftEyeViewport;
        [SerializeField] private int[] RightEyeViewport;

        // for monoscopic
        [SerializeField] private float[] CameraProjection;
        [SerializeField] private int[] RenderViewport;

        // deprecated
        [SerializeField] private Vector3 EyeCenterPosition;
#pragma warning restore CS0414

        protected abstract (int width, int height) defaultVideoResolution { get; }
        protected abstract float defaultVideoFrameRate { get; }
        protected abstract bool stereoscopy { get; }
        protected abstract RenderType renderType { get; }

        protected virtual string[] supportedVideoCodecs => AXRClientPlugin.GetSupportedVideoCodecs();
        protected virtual string[] supportedAudioCodecs => AXRClientPlugin.GetSupportedAudioCodecs();
        protected virtual bool hasInput => true;
        protected virtual bool isUserPresent => true;
        protected virtual bool isOpenglRenderTextureCoordInEditor => false;
        protected virtual bool useDedicatedRenderCamera => false;
        protected virtual float[] videoScale => new float[] { 1.0f, 1.0f }; // ratio of the size of the whole video rendered to the size of the area visible to an eye camera

        internal bool propHasInput => hasInput;
        internal bool propIsUserPresent => isUserPresent;
        internal bool propIsOpenglRenderTextureCoordInEditor => isOpenglRenderTextureCoordInEditor;
        internal bool propUseDedicatedRenderCamera => false;

        // for stereoscopic
        protected virtual float[] leftEyeCameraNearPlane => null;
        protected virtual float ipd => 0.0f;
        protected virtual int[] leftEyeViewport => null;
        protected virtual int[] rightEyeViewport => null;

        /*
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
        */

        // for monoscopic
        protected virtual float[] cameraProjection => null;
        protected virtual int[] renderViewport => null;

        internal bool useSeperateVideoRenderTarget => renderType == RenderType.VideoRenderTextureInScene;
        internal bool useSingleTextureForEyes => renderType == RenderType.VideoRenderTextureInScene;

        public string userID {
            get { return UserID; }
            set { UserID = value; }
        }

        public string place {
            get { return Place; }
            set { Place = value; }
        }

        public (int width, int height) videoResolution {
            get { return (VideoWidth, VideoHeight); }
            set {
                VideoWidth = value.width;
                VideoHeight = value.height;
            }
        }

        public float videoFrameRate {
            get { return VideoFrameRate > 0 ? VideoFrameRate : defaultVideoFrameRate; }
            set { VideoFrameRate = value; }
        }

        public int videoMinBitrate {
            get { return VideoMinBitrate; }
            set { VideoMinBitrate = value; }
        }

        public int videoStartBitrate {
            get { return VideoStartBitrate; }
            set { VideoStartBitrate = value; }
        }

        public int videoMaxBitrate {
            get { return VideoMaxBitrate; }
            set { VideoMaxBitrate = value; }
        }

        public AXRProfileBase() {
            TempPath = Application.persistentDataPath;
        }

        public override string ToString() {
            var resolution = videoResolution;

            return string.Format("[AXRProfile]\n" +
                                 "    videoWidth={0}\n" +
                                 "    videoHeight={1}\n" +
                                 "    videoFrameRate={2}\n" +
                                 "    videoScale=({3}, {4})\n" +
                                 "    renderType={5}\n" +
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

        internal AXRProfileBase GetSerializable() {
            SupportedVideoCodecs = supportedVideoCodecs;
            SupportedAudioCodecs = supportedAudioCodecs;
            Stereoscopy = stereoscopy;
            VideoScale = videoScale;

            if (VideoWidth <= 0 || VideoHeight <= 0) {
                var res = defaultVideoResolution;
                VideoWidth = res.width;
                VideoHeight = res.height;
            }
            if (VideoFrameRate <= 0.0f) {
                VideoFrameRate = defaultVideoFrameRate;
            }

            LeftEyeCameraNearPlane = leftEyeCameraNearPlane;
            EyeCenterPosition = eyeCenterPosition;
            IPD = ipd;
            LeftEyeViewport = leftEyeViewport;
            RightEyeViewport = rightEyeViewport;

            CameraProjection = cameraProjection;
            RenderViewport = renderViewport;

            return this;
        }

        // deprecated: unused anymore because it is only for 3-DOF tracking
        protected virtual Vector3 eyeCenterPosition => Vector3.zero;
    }
}
