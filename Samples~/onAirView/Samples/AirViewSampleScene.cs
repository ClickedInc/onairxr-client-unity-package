using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using onAirXR.Client;

public class AirViewSampleScene : MonoBehaviour, AXRClient.Context {
    private AXRClientState _state = AXRClientState.Idle;

    [SerializeField] private string _serverAddress = "";
    [SerializeField] private string _userID = "";
    [SerializeField] private string _place = "";

    private void Awake() {
        AXRClient.Configure(this);
    }

    private async void Start() {
        await Task.Delay(3000);

        AXRClient.StartLinking();
    }

    private void Update() {
        logClientStateChange();
    }

    private void logClientStateChange() {
        var next = AXRClient.state;
        if (next == _state) { return; }

        Debug.Log($"[onairxr] client state changed: {_state} -> {next}");
        _state = next;
    }

    AXRPlatform AXRClient.Context.platform => AXRPlatform.onAirXR;
    string AXRClient.Context.address => _serverAddress;
    bool AXRClient.Context.autoPlay => true;

    void AXRClient.Context.OnPreRequestLink(AXRProfileBase profile) {
        profile.userID = _userID;
        profile.place = _place;
    }

    void AXRClient.Context.OnLinked() {}
}
