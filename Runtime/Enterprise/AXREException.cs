using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace onAirXR.Client {
    public class AXREException : Exception {
        public enum Code : int {
            Unknown = 0,
            Network,
            HTTP,
            InvalidEndpoint,

            InvalidState = 1001,
            Busy,
            GroupNotFound,
            NoLinkageAvailable,
            GroupDeletedByOtherUser
        }

        public Code code { get; private set; }
        public long responseCode { get; private set; }
        public string reason { get; private set; }

        public AXREException(Code code, long responseCode, string reason) {
            this.code = code;
            this.responseCode = responseCode;
            this.reason = reason;
        }
    }
}
