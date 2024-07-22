using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public enum AXRClientState {
        Idle,
        WaitingForNextLinkageRequest,
        RequestingLinkage,
        Linking,
        Linked,
        RequestingPlay,
        Playing,
        Unlinking,
        Error
    }

    public enum AXRPlatform {
        onAirXR,
        Enterprise
    }

    public enum AXRLinkageRequestCase {
        Default,
        FirstRequest,
        UnlinkedByUser
    }

    public class AXRClientStateMachine {
        public interface Context {
            AXRPlatform platform { get; }
            string address { get; }
            bool autoPlay { get; }
            bool userPresent { get; }

            float EvalNextLinkageRequestDelay(AXRLinkageRequestCase reqcase);
            void RequestGetLinkage(string ipaddr, int port);
            void RequestLink(string ipaddr, int port);
            void RequestPlay();
            void RequestStop();
        }

        private Context _context;
        private State _state;
        private StateIdle _stateIdle;
        private StateWaitingForNextLinkageRequest _stateWaitingForNextLinkageRequest;
        private StateRequestingLinkage _stateRequestingLinkage;
        private StateLinking _stateLinking;
        private StateLinked _stateLinked;
        private StateRequestingPlay _stateRequestingPlay;
        private StatePlaying _statePlaying;
        private StateError _stateError;

        private bool appFocused { get; set; } = true;

        public AXRClientState state => _state.type;

        public AXRClientStateMachine(Context context) {
            _stateIdle = new StateIdle(this);
            _stateWaitingForNextLinkageRequest = new StateWaitingForNextLinkageRequest(this);
            _stateRequestingLinkage = new StateRequestingLinkage(this);
            _stateLinking = new StateLinking(this);
            _stateLinked = new StateLinked(this);
            _stateRequestingPlay = new StateRequestingPlay(this);
            _statePlaying = new StatePlaying(this);
            _stateError = new StateError(this);

            _context = context;
            _state = _stateIdle;
        }

        public void StartLinking(float delay) {
            _state.TriggerStartLinking(delay);
        }

        public void StopLinking() {
            _state.TriggerStopLinking();
        }

        public void ConnectLinkage(string ipaddr, int port) {
            _state.TriggerConnectLinkage(ipaddr, port);
        }

        public void TriggerLinked() {
            _state.TriggerLinked();
        }

        public void TriggerPlayResponded() {
            _state.TriggerPlayResponded();
        }

        public void TriggerUnlinkByUser() {
            _state.TriggerUnlinkByUser();
        }

        public void TriggerUnlinked() {
            _state.TriggerUnlinked();
        }

        public void Update(bool appFocused, float deltaTime) {
            this.appFocused = appFocused;

            _state.Update(deltaTime);
        }

        public void OnApplicationPause(bool pauseStatus) {
            this.appFocused = !pauseStatus;
            //_state.OnApplicationPause(pauseStatus);
        }

        private void transitTo(State to) {
            _state = to;
        }

        private abstract class State {
            private bool _unlinkRequestedByUser;

            protected AXRClientStateMachine owner { get; private set; }
            protected Context context => owner._context;

            public abstract AXRClientState type { get; }

            public State(AXRClientStateMachine owner) {
                this.owner = owner;
            }

            public virtual void TriggerStartLinking(float delay) {}
            public virtual void TriggerStopLinking() {}
            public virtual void TriggerConnectLinkage(string ipaddr, int port) {}
            public virtual void TriggerLinked() {}
            public virtual void TriggerPlayResponded() {}

            public virtual void TriggerUnlinkByUser() {
                _unlinkRequestedByUser = true;
            }

            public virtual void TriggerUnlinked() {
                if (context.autoPlay) {
                    owner.transitTo(owner._stateWaitingForNextLinkageRequest);

                    var reqcase = _unlinkRequestedByUser ? AXRLinkageRequestCase.UnlinkedByUser : AXRLinkageRequestCase.Default;
                    owner._stateWaitingForNextLinkageRequest.remainingToRequest = context.EvalNextLinkageRequestDelay(reqcase);
                }
                else {
                    owner.transitTo(owner._stateIdle);
                }

                _unlinkRequestedByUser = false;
            }

            public virtual void Update(float deltaTime) { }
            public virtual void OnApplicationPause(bool pauseStatus) {}

            protected void RequestGetLinkage() {
                if (AXRClient.TryParseAddress(context.address, out var ipaddr, out var port)) {
                    context.RequestGetLinkage(ipaddr, port);
                }
                else {
                    throw new UnityException("onairxr-invalid-address");
                }
            }

            protected void RequestLink() {
                if (AXRClient.TryParseAddress(context.address, out var ipaddr, out var port)) {
                    RequestLink(ipaddr, port);
                }
                else {
                    throw new UnityException("onairxr-invalid-address");
                }
            }

            protected void RequestLink(string ipaddr, int port) {
                context.RequestLink(ipaddr, port);
            }
        } 

        private class StateIdle : State {
            public override AXRClientState type => AXRClientState.Idle;

            public StateIdle(AXRClientStateMachine owner) : base(owner) { }

            public override void TriggerStartLinking(float delay) {
                owner.transitTo(owner._stateWaitingForNextLinkageRequest);
                owner._stateWaitingForNextLinkageRequest.remainingToRequest = delay > 0 ? delay : context.EvalNextLinkageRequestDelay(AXRLinkageRequestCase.FirstRequest);
            }

            public override void TriggerUnlinked() {}
        }  

        private class StateWaitingForNextLinkageRequest : State {
            private const float DelayToRequestWhenUserAbsent = 5.5f;

            public float remainingToRequest { private get; set; }
            public override AXRClientState type => AXRClientState.WaitingForNextLinkageRequest;

            public StateWaitingForNextLinkageRequest(AXRClientStateMachine owner) : base(owner) { }

            public override void TriggerStopLinking() {
                owner.transitTo(owner._stateIdle);
            }

            public override void Update(float deltaTime) {
                if (context.userPresent == false) {
                    remainingToRequest = DelayToRequestWhenUserAbsent;
                    return;
                }

                remainingToRequest -= deltaTime;
                if (remainingToRequest > 0) { return; }

                try {
                    switch (context.platform) {
                        case AXRPlatform.onAirXR:
                            RequestLink();
                            owner.transitTo(owner._stateLinking);
                            break;
                        case AXRPlatform.Enterprise:
                            RequestGetLinkage();
                            owner.transitTo(owner._stateRequestingLinkage);
                            break;
                    }
                }
                catch (Exception e) {
                    if ((e as UnityException)?.Message?.Equals("onairxr-invalid-address") ?? false) {
                        remainingToRequest = 1.0f;
                        return;
                    }

                    owner.transitTo(owner._stateError);
                    owner._stateError.error = e.Message;
                }
            }
        }

        private class StateRequestingLinkage : State {
            public override AXRClientState type => AXRClientState.RequestingLinkage;

            public StateRequestingLinkage(AXRClientStateMachine owner) : base(owner) { }

            public override void TriggerStopLinking() {
                owner.transitTo(owner._stateIdle);
            }

            public override void TriggerConnectLinkage(string ipaddr, int port) {
                RequestLink(ipaddr, port);
                owner.transitTo(owner._stateLinking);
            }
        }

        private class StateLinking : State {
            public override AXRClientState type => AXRClientState.Linking;

            public StateLinking(AXRClientStateMachine owner) : base(owner) { }

            public override void TriggerLinked() {
                owner.transitTo(owner._stateLinked);
                owner._stateLinked.SetTimer();
            }
        }

        private class StateLinked : State {
            private float _remainingToPlay;

            public override AXRClientState type => AXRClientState.Linked;

            public StateLinked(AXRClientStateMachine owner) : base(owner) { }

            public void SetTimer() {
                _remainingToPlay = 0.1f;
            }

            public override void Update(float deltaTime) {
                if (owner.appFocused == false || _remainingToPlay <= 0) { return; }

                _remainingToPlay -= deltaTime;
                if (_remainingToPlay > 0) { return; }

                owner.transitTo(owner._stateRequestingPlay);
                context.RequestPlay();
            }
        }

        private class StateRequestingPlay : State {
            public override AXRClientState type => AXRClientState.RequestingPlay;

            public StateRequestingPlay(AXRClientStateMachine owner) : base(owner) { }

            public override void TriggerPlayResponded() {
                owner.transitTo(owner._statePlaying);
            }
        }

        private class StatePlaying : State {
            public override AXRClientState type => AXRClientState.Playing;

            public StatePlaying(AXRClientStateMachine owner) : base(owner) { }

            public override void Update(float deltaTime) {
                if (owner.appFocused) { return; }

                context.RequestStop();
                
                owner.transitTo(owner._stateLinked);
                owner._stateLinked.SetTimer();
            }
        }

        private class StateError : State {
            public string error { get; set; }
            public override AXRClientState type => AXRClientState.Error;

            public StateError(AXRClientStateMachine owner) : base(owner) { }
        }
    }
}
