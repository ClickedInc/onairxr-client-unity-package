using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace onAirXR.Client {
    public class AXRClient : MonoBehaviour, AXRClientStateMachine.Context {
        public interface Context {
            AXRPlatform platform { get; }
            bool autoPlay { get; }
            string address { get; }
            string userid { get; }
            string place { get; }

            void OnLinked();
        }

        private static AXRClient _instance;
        private static Context _context;
        private static AXRClientEventDispatcher _eventDispatcher;

        public delegate void OnMessageReceiveHandler(AXRClientMessage message);
        public static event OnMessageReceiveHandler OnMessageReceived;

        public static AXRClientInputManager inputManager { get; private set; }
        public static bool configured => _instance?._configured ?? false;
        public static AXRClientState state => _instance?._stateMachine?.state ?? AXRClientState.Error;
        public static bool connected => state == AXRClientState.Linked ||
                                        state == AXRClientState.RequestingPlay || 
                                        state == AXRClientState.Playing;
        public static bool automaticallyPauseWhenUserNotPresent => (Application.platform != RuntimePlatform.WindowsEditor && 
                                                                    Application.platform != RuntimePlatform.WindowsPlayer) ||
                                                                   Application.runInBackground == false;
        public static string lastLinkageAddress => _instance?._lastLinkageAddress;

        public static bool TryParseAddress(string address, out string ipaddr, out int port) {
            ipaddr = null;
            port = 0;

            if (string.IsNullOrWhiteSpace(address)) { return false; }

            var parts = address.Split(':');
            if (parts.Length != 2) { return false; }

            if (string.IsNullOrWhiteSpace(parts[0]) ||
                int.TryParse(parts[1], out port) == false) { return false; }

            ipaddr = parts[0];
            return true;
        }

        public static void Configure(Context context) {
            _context = context;
        }

        public static void LoadOnce(AXRProfileBase profile, AXRCameraBase camera) {
            if (_instance != null) { return; }

            AXRClientPlugin.SetProfile(JsonUtility.ToJson(profile.GetSerializable()));

            var go = new GameObject("AXRClient");
            go.AddComponent<AXRClient>();
            Debug.Assert(_instance != null);

            _instance._profile = profile;
            _instance._camera = camera;
            if (profile.useSeperateVideoRenderTarget) {
                _instance._underlayVideoRenderer = new AXRUnderlayVideoRenderer(go, profile, camera);
            }

            inputManager = go.AddComponent<AXRClientInputManager>();
        }

        public static void StartLinking(float delay = -1f) {
            _instance?._stateMachine?.StartLinking(delay);
        }

        public static void StopLinking() {
            _instance?._stateMachine?.StopLinking();
        }

        public static void Unlink() {
            AXRClientPlugin.RequestDisconnect();
        }

        private AXREClient _enterpriseClient;
        private AXRProfileBase _profile;
        private AXRCameraBase _camera;
        private AXRUnderlayVideoRenderer _underlayVideoRenderer;
        private bool _configured;
        private AXRClientStateMachine _stateMachine;
        private string _lastLinkageAddress;

        private void Awake() {
            if (_instance != null) {
                // destroy myself if there is already an instance
                DestroyImmediate(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _enterpriseClient = new AXREClient();

            _eventDispatcher = new AXRClientEventDispatcher();
            _eventDispatcher.MessageReceived += onAXRMessageReceived;
        }

        private void Start() {
            if (_context == null) {
                throw new UnityException("[ERROR] AXRClient.Configure must be called on Awake().");
            }

            var result = AXRClientPlugin.Configure(AudioSettings.outputSampleRate, _profile.hasInput, isOpenglRenderTextureCoord());
            if (result < 0 && result != -4) { return; }
            
            _configured = true;
            _stateMachine = new AXRClientStateMachine(this, _profile.delayToResumePlayback);
        }

        private void Update() {
            _eventDispatcher.DispatchEvent();
            _stateMachine?.Update(!automaticallyPauseWhenUserNotPresent || _profile.isUserPresent, Time.deltaTime);
        }

        private void LateUpdate() {
            _underlayVideoRenderer?.LateUpdate();
        }

        private void OnApplicationPause(bool pauseStatus) {
            _stateMachine?.OnApplicationPause(pauseStatus);
        }

        private void OnDestroy() {
            _eventDispatcher.MessageReceived -= onAXRMessageReceived;

            AXRClientPlugin.Cleanup();
        }

        private bool isOpenglRenderTextureCoord() {
#if UNITY_EDITOR
            // NOTE: assumes that only Android build target is set to OpenGLES
            return EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
#else
            switch (SystemInfo.graphicsDeviceType) {
                case GraphicsDeviceType.OpenGL2:
                case GraphicsDeviceType.OpenGLES2:
                case GraphicsDeviceType.OpenGLES3:
                case GraphicsDeviceType.OpenGLCore:
                    return true;
                default:
                    return false;
            }
#endif
        }

        private void onAXRMessageReceived(AXRMessage message) {
            var msg = message as AXRClientMessage;
            Debug.Assert(msg != null);

            OnMessageReceived?.Invoke(msg);

            if (msg.IsSessionEvent()) {
                switch (msg.Name) {
                    case AXRClientMessage.NameSetupResponded:
                        onAXRSetupResponded(msg);
                        break;
                    case AXRClientMessage.NameRenderPrepared:
                        onAXRRenderPrepared(msg);
                        break;
                    case AXRClientMessage.NamePlayResponded:
                        onAXRPlayResponded(msg);
                        break;
                    case AXRClientMessage.NameStopResponded:
                        onAXRStopResponded(msg);
                        break;
                    case AXRClientMessage.NameDisconnected:
                        onAXRDisconnected(msg);
                        break;
                }
            }
            else if (message.Type == AXRMessage.TypeUserData) {
                onAXRUserDataReceived(msg);
            }
        }

        private void onAXRSetupResponded(AXRClientMessage message) {
            AXRClientPlugin.PrepareRender();
        }

        private void onAXRRenderPrepared(AXRClientMessage message) {
            if (_underlayVideoRenderer != null &&
                AXRClientPlugin.GetVideoRenderTargetTexture(out var nativeTex, out var width, out var height)) {
                var texture = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, nativeTex);
                _underlayVideoRenderer.SetVideoFrameTexture(texture);
                _underlayVideoRenderer.enabled = true;
            }

            _stateMachine?.TriggerLinked(); 
            _context.OnLinked();
        }

        private void onAXRPlayResponded(AXRClientMessage message) {
            _stateMachine?.TriggerPlayResponded();
        }

        private void onAXRStopResponded(AXRClientMessage message) {
            
        }

        private void onAXRDisconnected(AXRClientMessage message) {
            if (_underlayVideoRenderer != null) {
                _underlayVideoRenderer.SetVideoFrameTexture(null);
                _underlayVideoRenderer.enabled = false;
            }

            _stateMachine?.TriggerUnlinked();
        }

        private void onAXRUserDataReceived(AXRClientMessage message) {
            // TODO handle user data, in statemachine?
        }

        // implements AXRClientStateMachine.Context
        AXRPlatform AXRClientStateMachine.Context.platform => _context.platform;
        string AXRClientStateMachine.Context.address => _context.address;
        bool AXRClientStateMachine.Context.autoPlay => _context.autoPlay;

        float AXRClientStateMachine.Context.EvalNextLinkageRequestDelay(bool lazyStart) {
            if (_context.autoPlay) {
                return 1.0f + 0.5f * UnityEngine.Random.value;
            }
            
            return lazyStart ? 0.75f : -1f;
        }

        async void AXRClientStateMachine.Context.RequestGetLinkage(string ipaddr, int port) {
            try {
                _enterpriseClient.SetAddress(ipaddr, port);

                var linkage = await _enterpriseClient.GetLinkage();
                if (linkage == null) { throw new AXREException(AXREException.Code.NoLinkageAvailable, 0, "not found"); }

                _stateMachine.ConnectLinkage(linkage.address, linkage.port);
            }
            catch (Exception e) {
                Debug.Log($"[onairxr] axrclient: failed to get linkage from {ipaddr}:{port} : {e.Message}");
                _stateMachine.TriggerUnlinked();
            }
        }

        void AXRClientStateMachine.Context.RequestLink(string ipaddr, int port) {
            _lastLinkageAddress = $"{ipaddr}:{port}";

            _instance._camera.OnPreLink();
            AXRClientPlugin.SetProfile(JsonUtility.ToJson(_profile.GetSerializable()));

            AXRClientPlugin.RequestConnect(ipaddr, port);
        }

        void AXRClientStateMachine.Context.RequestPlay() {
            AXRClientPlugin.RequestPlay();
        }

        void AXRClientStateMachine.Context.RequestStop() {
            AXRClientPlugin.RequestStop();
        }
    }   
}
