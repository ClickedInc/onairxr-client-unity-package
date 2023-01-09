/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Assertions;
using onAirXR.Client;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AirViewClient : MonoBehaviour {
    public interface EventHandler {
        void AirViewClientFailed(string reason);
        void AirViewClientConnected();
        void AirViewClientPlaybackStarted();
        void AirViewClientPlaybackStopped();
        void AirViewClientDisconnected();
    }

    internal delegate void OnAirViewMessageReceiveHandler(AirViewMessage message);
    internal static event OnAirViewMessageReceiveHandler MessageReceived;    

    public static AirViewClient instance { get; private set; }
    public static EventHandler Delegate { private get; set; }

    public static AirViewInputManager input => instance != null ? instance._input : null;
    public static bool configured => instance != null ? instance._configured : false;
    public static bool connected => instance != null ? instance.IsConnected() : false;
    public static bool playing => instance != null ? instance.IsPlaying() : false;

    public static void LoadOnce(AirViewCamera camera) {
        if (instance != null) { return; }

        var go = new GameObject("AirViewClient");
        go.AddComponent<AirViewClient>();

        instance.setProfile(camera.profile);
    }

    public static void Connect(AirViewCamera camera, string address, int port) {
        if (instance == null) { return; }

        instance.setProfile(camera.profile);
        instance.RequestConnect(address, port);
    }

    public static void Play() {
        if (instance == null) { return; }

        instance.RequestPlay();
    }

    public static void Stop() {
        if (instance == null) { return; }

        instance.RequestStop();
    }

    public static void Disconnect() {
        if (instance == null) { return; }

        instance.RequestDisconnect();
    }

    public static void SendUserData(byte[] data) {
        if (instance == null) { return; }

        instance.RequestSendUserData(data);
    }

    private AirViewInputManager _input;
    private AirViewEventDispatcher _eventDispatcher;
    private Statemachine _statemachine;
    private bool _configured;

    internal AirViewProfile profile { get; private set; }

    public bool IsConnected() {
        return AXRClientPlugin.IsConnected();
    }

    public bool IsPlaying() {
        return AXRClientPlugin.IsPlaying();
    }

    public void RequestConnect(string address, int port) {
        AXRClientPlugin.RequestConnect(address, port);
    }

    public void RequestPlay() {
        _statemachine.TriggerPlayRequested();
    }

    public void RequestStop() {
        _statemachine.TriggerStopRequested();
    }

    public void RequestDisconnect() {
        AXRClientPlugin.RequestDisconnect();
    }

    public void RequestSendUserData(byte[] data) {
        AXRClientPlugin.RequestSendUserData(data);
    }

    private void Awake() {
        if (instance != null) { throw new UnityException("[ERROR] there must exist only one instance of AirViewClient"); }

        instance = this;
        _input = gameObject.AddComponent<AirViewInputManager>();
        DontDestroyOnLoad(gameObject);

        _eventDispatcher = new AirViewEventDispatcher();
        _eventDispatcher.MessageReceived += onAirViewMessageReceived;
    }

    private void Start() {
        if (Delegate == null) { throw new UnityException("[ERROR] you must set AirViewClient.Delegate"); }

        var result = AXRClientPlugin.Configure(AudioSettings.outputSampleRate, true, isOpenglRenderTextureCoord());
        if (result < 0 && result != -4) {
            Delegate.AirViewClientFailed("[ERROR] failed to init AirViewClient : " + result);
        }
        else {
            _configured = true;
        }

        _statemachine = new Statemachine(this, 0.1f);
    }

    private void Update() {
        _eventDispatcher.DispatchEvent();
        _statemachine.Update(true, Time.deltaTime);
    }

    private void OnApplicationPause(bool pause) {
        _statemachine.UpdatePauseStatus(pause);
    }

    private void OnDestroy() {
        _eventDispatcher.MessageReceived -= onAirViewMessageReceived;

        AXRClientPlugin.Cleanup();
    }

    private void setProfile(AirViewProfile profile) {
        this.profile = profile;

        AXRClientPlugin.SetProfile(profile.Serialize());
    }

    private void onAirViewMessageReceived(AXRMessage message) {
        var msg = message as AirViewMessage;
        Assert.IsNotNull(msg);

        MessageReceived(msg);

        if (msg.IsSessionEvent()) {
            if (msg.Name.Equals(AirViewMessage.NameSetupResponded)) {
                onSetupResponded(msg);
            }
            else if (msg.Name.Equals(AirViewMessage.NameRenderPrepared)) {
                onRenderPrepared(msg);
            }
            else if (msg.Name.Equals(AirViewMessage.NamePlayResponded)) {
                onPlayResponded(msg);
            }
            else if (msg.Name.Equals(AirViewMessage.NameStopResponded)) {
                onStopResponded(msg);
            }
            else if (msg.Name.Equals(AirViewMessage.NameDisconnected)) {
                onDisconnected(msg);
            }
        }
    }

    private void onSetupResponded(AirViewMessage message) {
        AXRClientPlugin.PrepareRender();
    }

    private void onRenderPrepared(AirViewMessage message) {
        _statemachine.TriggerConnected();
        Delegate.AirViewClientConnected();
    }

    private void onPlayResponded(AirViewMessage message) {
        Delegate.AirViewClientPlaybackStarted();
    }

    private void onStopResponded(AirViewMessage message) {
        Delegate.AirViewClientPlaybackStopped();
    }

    private void onDisconnected(AirViewMessage message) {
        _statemachine.TriggerDisconnected();
        Delegate.AirViewClientDisconnected();
    }

    private void statemachineRequestPlay() {
        AXRClientPlugin.RequestPlay();
    }

    private void statemachineRequestStop() {
        AXRClientPlugin.RequestStop();
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

    private class Statemachine {
        private AirViewClient _context;
        private State _state;
        private bool _lastAppFocused = true;

        protected StateDisconnected stateDisconnected { get; private set; }
        protected StateReady stateReady { get; private set; }
        protected StateUnfocused stateUnfocused { get; private set; }
        protected StatePlaying statePlaying { get; private set; }
        protected StateInactive stateInactive { get; private set; }
        protected StatePaused statePaused { get; private set; }
        protected StateResuming stateResuming { get; private set; }

        public Statemachine(AirViewClient context, float delayedResume) {
            stateDisconnected = new StateDisconnected(this);
            stateReady = new StateReady(this);
            stateUnfocused = new StateUnfocused(this);
            statePlaying = new StatePlaying(this);
            stateInactive = new StateInactive(this);
            statePaused = new StatePaused(this);
            stateResuming = new StateResuming(this, delayedResume);

            _context = context;
            _state = stateDisconnected;
        }

        protected void TransitTo(State state) {
            _state = state;
        }

        public void TriggerConnected() {
            _state.Connected(_context, _lastAppFocused);
        }

        public void TriggerPlayRequested() {
            _state.PlayRequested(_context);
        }

        public void TriggerStopRequested() {
            _state.StopRequested(_context);
        }

        public void TriggerDisconnected() {
            _state.Disconnected(_context);
        }

        public void Update(bool appFocused, float deltaTime) {
            if (_lastAppFocused != appFocused) {
                if (appFocused) {
                    _state.AppFocused(_context);
                }
                else {
                    _state.AppUnfocused(_context);
                }
                _lastAppFocused = appFocused;
            }

            _state.Update(_context, deltaTime);
        }

        public void UpdatePauseStatus(bool pauseStatus) {
            if (pauseStatus) {
                _state.AppPaused(_context);
            }
            else {
                _state.AppResumed(_context);
            }
        }

        protected abstract class State {
            protected Statemachine owner { get; private set; }

            public State(Statemachine owner) {
                this.owner = owner;
            }

            public virtual void Connected(AirViewClient context, bool appFocused) { Assert.IsTrue(false); }
            public virtual void PlayRequested(AirViewClient context) { }
            public virtual void StopRequested(AirViewClient context) { }
            public virtual void AppFocused(AirViewClient context) { }
            public virtual void AppUnfocused(AirViewClient context) { }
            public virtual void AppPaused(AirViewClient context) { }
            public virtual void AppResumed(AirViewClient context) { }
            public virtual void Update(AirViewClient context, float deltaTime) { }

            public virtual void Disconnected(AirViewClient context) {
                owner.TransitTo(owner.stateDisconnected);
            }
        }

        protected class StateDisconnected : State {
            public StateDisconnected(Statemachine owner) : base(owner) { }

            public override void Connected(AirViewClient context, bool appFocused) {
                owner.TransitTo(appFocused ? owner.stateReady as State : owner.stateUnfocused as State);
            }

            public override void Disconnected(AirViewClient context) { }
        }

        protected class StateReady : State {
            public StateReady(Statemachine owner) : base(owner) { }

            public override void PlayRequested(AirViewClient context) {
                owner.TransitTo(owner.statePlaying);
                context.statemachineRequestPlay();
            }

            public override void AppUnfocused(AirViewClient context) {
                owner.TransitTo(owner.stateUnfocused);
            }
        }

        protected class StateUnfocused : State {
            public StateUnfocused(Statemachine owner) : base(owner) { }

            public override void AppFocused(AirViewClient context) {
                owner.TransitTo(owner.stateReady);
            }

            public override void PlayRequested(AirViewClient context) {
                owner.TransitTo(owner.stateInactive);
            }
        }

        protected class StatePlaying : State {
            public StatePlaying(Statemachine owner) : base(owner) { }

            public override void StopRequested(AirViewClient context) {
                owner.TransitTo(owner.stateReady);
                context.statemachineRequestStop();
            }

            public override void AppUnfocused(AirViewClient context) {
                owner.TransitTo(owner.stateInactive);
                context.statemachineRequestStop();
            }

            public override void AppPaused(AirViewClient context) {
                owner.TransitTo(owner.statePaused);
                context.statemachineRequestStop();
            }
        }

        protected class StateInactive : State {
            public StateInactive(Statemachine owner) : base(owner) { }

            public override void StopRequested(AirViewClient context) {
                owner.TransitTo(owner.stateUnfocused);
            }

            public override void AppFocused(AirViewClient context) {
                owner.TransitTo(owner.statePlaying);
                context.statemachineRequestPlay();
            }

            public override void AppPaused(AirViewClient context) {
                owner.TransitTo(owner.statePaused);
            }
        }

        protected class StatePaused : State {
            public StatePaused(Statemachine owner) : base(owner) { }

            public override void AppResumed(AirViewClient context) {
                owner.TransitTo(owner.stateResuming);
            }
        }

        protected class StateResuming : State {
            private float _delay;
            private float _remainingToResume;

            public StateResuming(Statemachine owner, float delay) : base(owner) {
                _delay = delay;
                _remainingToResume = delay;
            }

            public override void Update(AirViewClient context, float deltaTime) {
                _remainingToResume -= deltaTime;
                if (_remainingToResume <= 0.0f) {
                    _remainingToResume = _delay;

                    owner.TransitTo(owner.statePlaying);
                    context.statemachineRequestPlay();
                }
            }

            public override void StopRequested(AirViewClient context) {
                _remainingToResume = _delay;
                owner.TransitTo(owner.stateReady);
            }

            public override void AppUnfocused(AirViewClient context) {
                _remainingToResume = _delay;
                owner.TransitTo(owner.stateInactive);
            }

            public override void Disconnected(AirViewClient context) {
                _remainingToResume = _delay;

                base.Disconnected(context);
            }
        }
    }
}
