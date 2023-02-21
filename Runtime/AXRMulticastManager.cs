/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace onAirXR.Client {
    public class AXRMulticastManager : MonoBehaviour {
        public interface EventListener {
            void MemberJoined(AXRMulticastManager manager, string member, byte subgroup);
            void MemberChangedMembership(AXRMulticastManager manager, string member, byte subgroup);
            void MemberLeft(AXRMulticastManager manager, string member);
            void GetInputsPerFrame(AXRMulticastManager manager);
            bool PendInputsPerFrame(AXRMulticastManager manager);
        }

        private static AXRMulticastManager _instance;

#if UNITY_IOS
    private const string LibName = "__Internal";
#else
        private const string LibName = "axr";
#endif

        [DllImport(LibName)] private static extern void axr_MulticastEnumerateIPv6Interfaces(ref StringBuffer result);
        [DllImport(LibName)] private static extern int axr_MulticastStartup(string address, int port, string netaddr);
        [DllImport(LibName)] private static extern void axr_MulticastShutdown();
        [DllImport(LibName)] private static extern bool axr_MulticastCheckMessageQueue(out ulong source, out IntPtr data, out int length);
        [DllImport(LibName)] private static extern void axr_MulticastRemoveFirstMessageFromQueue();
        [DllImport(LibName)] private static extern void axr_MulticastJoin();
        [DllImport(LibName)] private static extern void axr_MulticastLeave();
        [DllImport(LibName)] private static extern void axr_MulticastSetSubgroup(byte subgroup);
        [DllImport(LibName)] private static extern long axr_MulticastBeginPendInput();
        [DllImport(LibName)] private static extern void axr_MulticastPendInputByteStream(byte device, byte control, byte value);
        [DllImport(LibName)] private static extern void axr_MulticastPendInputPose(byte device, byte control, AXRVector3D position, AXRVector4D rotation);
        [DllImport(LibName)] private static extern void axr_MulticastSendPendingInputs(long timestamp);
        [DllImport(LibName)] private static extern long axr_MulticastGetInputRecvTimestamp(string member);
        [DllImport(LibName)] private static extern bool axr_MulticastGetInputByteStream(string member, byte device, byte control, ref byte value);
        [DllImport(LibName)] private static extern bool axr_MulticastGetInputPose(string member, byte device, byte control, ref AXRVector3D position, ref AXRVector4D rotation);
        [DllImport(LibName)] private static extern void axr_MulticastUpdate();

        public static void LoadOnce(string address, int port, string hint = null, bool leaveOnStartup = false) {
            if (_instance != null || string.IsNullOrEmpty(address)) { return; }

            _instance = new GameObject("AXRMulticastManager").AddComponent<AXRMulticastManager>();
            DontDestroyOnLoad(_instance.gameObject);

            _instance._address = address;
            _instance._port = port;
            _instance._leaveOnStartup = leaveOnStartup;

            if (string.IsNullOrEmpty(hint) == false) {
                if (hint.Split('.').Length == 4) { // IPv4 net address
                    _instance._netaddr = hint;
                }
                else if (Application.isEditor == false &&
                         (Application.platform == RuntimePlatform.Android ||
                          Application.platform == RuntimePlatform.IPhonePlayer)) { // IPv6 interface name for Android
                    var inf = searchIPv6Interface(hint);
                    if (inf.isValid) {
                        _instance._netaddr = string.Format("{0}/128", inf.Address);
                    }
                }
            }
        }

        public static bool joined => _instance != null ? _instance._state == State.Joined : false;

        public static void RegisterDelegate(EventListener aDelegate) {
            _instance?.registerDelegate(aDelegate);
        }

        public static void Join() {
            _instance?.join();
        }

        public static void Leave() {
            _instance?.leave();
        }

        public static void SetSubgroup(byte subgroup) {
            _instance?.setSubgroup(subgroup);
        }

        private static IPv6Interface searchIPv6Interface(string name) {
            var builder = new StringBuilder(1024);
            builder.Append((char)0);
            builder.Append('*', builder.Capacity - 8);

            var strbuf = new StringBuffer(builder);
            axr_MulticastEnumerateIPv6Interfaces(ref strbuf);

            var interfaces = JsonUtility.FromJson<IPv6Interfaces>("{\"interfaces\":" + strbuf.buffer + "}");
            foreach (var inf in interfaces.interfaces) {
                if (inf.Name.Equals(name)) {
                    return inf;
                }
            }
            return new IPv6Interface();
        }

        private string _address;
        private int _port;
        private string _netaddr;
        private bool _leaveOnStartup;
        private EventListener _delegate;
        private State _state = State.Uninitialized;
        private byte[] _msgbuf = new byte[1024];
        private Dictionary<string, byte> _members = new Dictionary<string, byte>();
        private AndroidJavaObject _multicastLock;

        public long GetInputRecvTimestamp(string member) {
            if (_state == State.Uninitialized) { return -1; }

            return axr_MulticastGetInputRecvTimestamp(member);
        }

        public bool GetInputByteStream(string member, byte device, byte control, ref byte value) {
            if (_state == State.Uninitialized) { return false; }

            return axr_MulticastGetInputByteStream(member, device, control, ref value);
        }

        public bool GetInputPose(string member, byte device, byte control, ref Vector3 position, ref Quaternion rotation) {
            if (_state == State.Uninitialized) { return false; }

            var pos = new AXRVector3D();
            var rot = new AXRVector4D();

            if (axr_MulticastGetInputPose(member, device, control, ref pos, ref rot) == false) { return false; }

            position = pos.toVector3();
            rotation = rot.toQuaternion();
            return true;
        }

        public void PendInputByteStream(byte device, byte control, byte value) {
            if (_state == State.Uninitialized) { return; }

            axr_MulticastPendInputByteStream(device, control, value);
        }

        public void PendInputPose(byte device, byte control, Vector3 position, Quaternion rotation) {
            if (_state == State.Uninitialized) { return; }

            axr_MulticastPendInputPose(device, control, new AXRVector3D(position), new AXRVector4D(rotation));
        }

        private void Start() {
            acquireMulticastLock();

            var ret = (ErrorCode)axr_MulticastStartup(_address, _port, _netaddr);
            if (ret != ErrorCode.NoError) {
                Debug.LogWarningFormat("[WARNING] failed to startup OCSMulticastManager : {0}", ret);
                return;
            }

            Debug.LogFormat("[onAirXR Multicast] started up: {0} on port {1} (netaddr = {2})", _address, _port, _netaddr ?? "null");

            _state = State.Joined;

            if (_leaveOnStartup) {
                leave();
            }
        }

        private void Update() {
            if (_state != State.Uninitialized) {
                axr_MulticastUpdate();
                dispatchMessage();
            }

            _delegate?.GetInputsPerFrame(this);
        }

        private void LateUpdate() {
            if (_delegate == null || _state != State.Joined) { return; }

            var timestamp = axr_MulticastBeginPendInput();
            if (_delegate.PendInputsPerFrame(this)) {
                axr_MulticastSendPendingInputs(timestamp);
            }
        }

        private void OnDestroy() {
            if (_state == State.Uninitialized) { return; }

            axr_MulticastShutdown();

            releaseMulticastLock();
        }

        private void acquireMulticastLock() {
            if (Application.isEditor || Application.platform != RuntimePlatform.Android) { return; }

            var wifiManager = getWifiManager();
            if (wifiManager == null) { return; }

            _multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", "kr.co.clicked.onairxr.multicast");
            _multicastLock?.Call("acquire");
        }

        private void releaseMulticastLock() {
            _multicastLock?.Call("release");
        }

        private AndroidJavaObject getWifiManager() {
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            if (activity == null) { return null; }

            return activity.Call<AndroidJavaObject>("getSystemService", "wifi");
        }

        private void registerDelegate(EventListener aDelegate) {
            _delegate = aDelegate;

            foreach (var member in _members.Keys) {
                _delegate?.MemberJoined(this, member, _members[member]);
            }
        }

        private void join() {
            if (_state != State.Ready) { return; }

            axr_MulticastJoin();
            _state = State.Joined;
        }

        private void leave() {
            if (_state != State.Joined) { return; }

            axr_MulticastLeave();

            foreach (var member in _members.Keys) {
                _delegate?.MemberLeft(this, member);
            }
            _members.Clear();

            _state = State.Ready;
        }

        private void setSubgroup(byte subgroup) {
            if (_state == State.Uninitialized) { return; }

            axr_MulticastSetSubgroup(subgroup);
        }

        private void dispatchMessage() {
            while (axr_MulticastCheckMessageQueue(out ulong source, out IntPtr data, out int length)) {
                Marshal.Copy(data, _msgbuf, 0, length);
                axr_MulticastRemoveFirstMessageFromQueue();

                var message = JsonUtility.FromJson<Message>(Encoding.UTF8.GetString(_msgbuf, 0, length));
                Assert.IsNotNull(message.Type);

                switch (message.Type) {
                    case Message.TypeJoin:
                        handleMessageJoin(source, message);
                        break;
                    case Message.TypeChangeMembership:
                        handleMessageChangeMembership(source, message);
                        break;
                    case Message.TypeLeave:
                        handleMessageLeave(source, message);
                        break;
                }
            }
        }

        private void handleMessageJoin(ulong source, Message message) {
            Assert.IsFalse(string.IsNullOrEmpty(message.Member));
            Assert.IsFalse(_members.ContainsKey(message.Member));

            var subgroup = (byte)message.Subgroup;
            _members.Add(message.Member, subgroup);

            _delegate?.MemberJoined(this, message.Member, subgroup);
        }

        private void handleMessageChangeMembership(ulong source, Message message) {
            Assert.IsFalse(string.IsNullOrEmpty(message.Member));
            Assert.IsTrue(_members.ContainsKey(message.Member));

            var subgroup = (byte)message.Subgroup;
            _members[message.Member] = subgroup;

            _delegate?.MemberChangedMembership(this, message.Member, subgroup);
        }

        private void handleMessageLeave(ulong source, Message message) {
            Assert.IsFalse(string.IsNullOrEmpty(message.Member));
            Assert.IsTrue(_members.ContainsKey(message.Member));

            _members.Remove(message.Member);

            _delegate?.MemberLeft(this, message.Member);
        }

        private enum State {
            Uninitialized,
            Ready,
            Joined
        }

        private enum ErrorCode : int {
            NoError = 0,
            InvalidMulticastAddress = -1,
            NoMulticastInterface = -2,
            NoCorrespondingInterface = -3,
            MoreThanOneInterface = -4
        }

        #pragma warning disable 0649

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct StringBuffer {
            public string buffer;
            public int size;

            public StringBuffer(StringBuilder builder) {
                buffer = builder.ToString();
                size = buffer.Length;
            }
        }

        [Serializable]
        private struct IPv6Interfaces {
            public IPv6Interface[] interfaces;
        }

        [Serializable]
        private struct IPv6Interface {
            public string Name;
            public string Address;

            public bool isValid => string.IsNullOrEmpty(Name) == false && string.IsNullOrEmpty(Address) == false;
        }

        [Serializable]
        private struct Message {
            public const string TypeJoin = "join";
            public const string TypeLeave = "leave";
            public const string TypeChangeMembership = "change-membership";
            public const string TypeError = "error";

            // common
            public string Type;

            // for Type == join / change-membership / leave
            public string Member;

            // for Type == join / change-membership
            public int Subgroup;

            // for Type == error
            public string Source;
            public int ErrorCode;
        }

        #pragma warning restore 0649
    }
}
