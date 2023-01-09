/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;
using UnityEngine;
using UnityEngine.Assertions;
using onAirXR.Client;

[Serializable]
public class AirViewMessage : AXRMessage {
    // Type : Event
    public const string FromSession = "Session";

    // Type : Event, From : Session
    public const string NameConnected = "Connected";
    public const string NameSetupResponded = "SetupResponded";
    public const string NameRenderPrepared = "RenderPrepared";
    public const string NamePlayResponded = "PlayResponded";
    public const string NameStopResponded = "StopResponded";
    public const string NameDisconnected = "Disconnected";

    // Type : Event
    public string From;
    public string Name;

    public static AirViewMessage Parse(IntPtr source, string message) {
        var result = JsonUtility.FromJson<AirViewMessage>(message);

        Assert.IsFalse(string.IsNullOrEmpty(result.Type));
        result.source = source;
        result.postParse();

        return result;
    }

    public bool IsSessionEvent()        { return isEventFrom(FromSession); }

    private bool isEventFrom(string fromWhat) {
        if (string.IsNullOrEmpty(Type) || string.IsNullOrEmpty(From) || string.IsNullOrEmpty(Name)) { return false; }

        return Type.Equals(TypeEvent) && From.Equals(fromWhat);
    }
}
