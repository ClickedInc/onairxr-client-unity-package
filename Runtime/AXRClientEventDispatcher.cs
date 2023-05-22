using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public class AXRClientEventDispatcher : AXREventDispatcher {
        protected override AXRMessage ParseMessageImpl(IntPtr source, string message) {
            return AXRClientMessage.Parse(source, message);
        }

        protected override bool CheckMessageQueueImpl(out IntPtr source, out IntPtr data, out int length) {
            return AXRClientPlugin.CheckMessageQueue(out source, out data, out length);
        }

        protected override void RemoveFirstMessageFromQueueImpl() {
            AXRClientPlugin.RemoveFirstMessageFromQueue();
        }

        internal void SendUserData(byte[] data) {
            AXRClientPlugin.RequestSendUserData(data);
        }
    }
}
