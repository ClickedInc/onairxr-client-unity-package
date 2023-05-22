using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public class AXRClientInputManager : MonoBehaviour {
        private AXRClientInputStream _inputStream;

        public void Register(AXRInputSender sender) {
            _inputStream.RegisterInputSender(sender);
        }

        public void Unregister(AXRInputSender sender) {
            _inputStream.UnregisterInputSender(sender);
        }

        private void Awake() {
            _inputStream = new AXRClientInputStream();

            AXRClient.OnMessageReceived += onAXRMessageReceived;
        }

        private void Update() {
            _inputStream.UpdateInputFrame();
        }

        private void LateUpdate() {
            _inputStream.UpdateSenders();
        }

        private void OnDestroy() {
            AXRClient.OnMessageReceived -= onAXRMessageReceived;
        }

        private void onAXRMessageReceived(AXRClientMessage message) {
            if (message.IsSessionEvent() == false) { return; }

            switch (message.Name) {
                case AXRClientMessage.NameSetupResponded:
                    _inputStream.Init();
                    break;
                case AXRClientMessage.NamePlayResponded:
                    _inputStream.Start();
                    break;
                case AXRClientMessage.NameStopResponded:
                    _inputStream.Stop();
                    break;
                case AXRClientMessage.NameDisconnected:
                    _inputStream.Cleanup();
                    break;
            }
        }
    }
}

