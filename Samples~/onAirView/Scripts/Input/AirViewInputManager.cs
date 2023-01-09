/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

public class AirViewInputManager : MonoBehaviour {
    private AXRInputStream _inputStream;

    public void RegisterInputSender(AXRInputSender sender) {
        _inputStream.RegisterInputSender(sender);
    }

    public void UnregisterInputSender(AXRInputSender sender) {
        _inputStream.UnregisterInputSender(sender);
    }

    private void Awake() {
        _inputStream = new AirViewInputStream();

        AirViewClient.MessageReceived += onAirViewMessageReceived;
    }

    private void Update() {
        _inputStream.UpdateInputFrame();
    }

    private void LateUpdate() {
        _inputStream.UpdateSenders();
    }

    private void OnDestroy() {
        AirViewClient.MessageReceived -= onAirViewMessageReceived;
    }

    private void onAirViewMessageReceived(AirViewMessage message) {
        if (message.IsSessionEvent()) {
            if (message.Name.Equals(AirViewMessage.NameSetupResponded)) {
                _inputStream.Init();
            }
            else if (message.Name.Equals(AirViewMessage.NamePlayResponded)) {
                _inputStream.Start();
            }
            else if (message.Name.Equals(AirViewMessage.NameStopResponded)) {
                _inputStream.Stop();
            }
            else if (message.Name.Equals(AirViewMessage.NameDisconnected)) {
                _inputStream.Cleanup();
            }
        }
    }
}
