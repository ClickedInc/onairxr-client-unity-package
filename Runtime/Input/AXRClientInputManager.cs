using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public class AXRClientInputManager : MonoBehaviour {
        internal AXRClientInputStream inputStream { get; private set; }

        public void Register(AXRInputSender sender) {
            inputStream.RegisterInputSender(sender);
        }

        public void Unregister(AXRInputSender sender) {
            inputStream.UnregisterInputSender(sender);
        }

        private void Awake() {
            inputStream = new AXRClientInputStream();

            AXRClient.OnMessageReceived += onAXRMessageReceived;
        }

        private void Update() {
            inputStream.UpdateInputFrame();
        }

        private void LateUpdate() {
            inputStream.UpdateSenders();
        }

        private void OnDestroy() {
            AXRClient.OnMessageReceived -= onAXRMessageReceived;
        }

        private void onAXRMessageReceived(AXRClientMessage message) {
            if (message.IsSessionEvent() == false) { return; }

            switch (message.Name) {
                case AXRClientMessage.NameSetupResponded:
                    inputStream.Init();
                    break;
                case AXRClientMessage.NamePlayResponded:
                    inputStream.Start();
                    break;
                case AXRClientMessage.NameStopResponded:
                    inputStream.Stop();
                    break;
                case AXRClientMessage.NameDisconnected:
                    inputStream.Cleanup();
                    break;
            }
        }
    }
}

