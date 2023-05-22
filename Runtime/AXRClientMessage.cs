using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public class AXRClientMessage : AXRMessage {
        // Type : Event
        public const string FromSession = "Session";
        public const string FromMediaStream = "MediaStream";
        public const string FromInputStream = "InputStream";

        // Type : Event, From : Session
        public const string NameConnected = "Connected";
        public const string NameSetupResponded = "SetupResponded";
        public const string NameRenderPrepared = "RenderPrepared";
        public const string NamePlayResponded = "PlayResponded";
        public const string NameStopResponded = "StopResponded";
        public const string NameDisconnected = "Disconnected";

        // Type : Event, From : MediaStream
        public const string NameCameraClipPlanes = "CameraClipPlanes";
        public const string NameEnableNetworkTimeWarp = "EnableNetworkTimeWarp";

        // Type : Event, From : InputStream
        public const string NameRecenterPose = "RecenterPose";

        public static AXRClientMessage Parse(IntPtr source, string message) {
            var result = JsonUtility.FromJson<AXRClientMessage>(message);

            Debug.Assert(string.IsNullOrEmpty(result.Type) == false);
            result.source = source;
            result.postParse();

            return result;
        }

        // Type : Event
        public string From;
        public string Name;

        // Type : Event, From : MediaStream, Name : CameraClipPlanes
        public float NearClip;
        public float FarClip;

        // Type : Event, From : MediaStream, Name : EnableTimeWarp
        public bool Enable;

        // Type : Event, From : InputStream, Name : RemoteInputDeviceRegistered / RemoteInputDeviceUnregistered
        public int DeviceID;

        // Type : Event, From : InputStream, Name : RemoteInputDeviceRegistered
        [SerializeField]
        protected string PointerCookieTexture;
        public byte[] PointerCookieTexture_Decoded { get; private set; }
        public string DeviceName;
        public float PointerCookieDepthScaleMultiplier;

        protected override void postParse() {
            base.postParse();
            if (string.IsNullOrEmpty(PointerCookieTexture) == false) {
                PointerCookieTexture_Decoded = Convert.FromBase64String(PointerCookieTexture);
            }
        }

        private bool isEventFrom(string fromWhat) {
            if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(From) || string.IsNullOrEmpty(Name)) {
                return false;
            }
            return Type.Equals(TypeEvent) && From.Equals(fromWhat);
        }

        public bool IsSessionEvent() { return isEventFrom(FromSession); }
        public bool IsMediaStreamEvent() { return isEventFrom(FromMediaStream); }
        public bool IsInputStreamEvent() { return isEventFrom(FromInputStream); }
    }
}
