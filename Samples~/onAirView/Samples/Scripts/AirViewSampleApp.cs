/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/* public class AirViewSampleApp : MonoBehaviour, AirViewClient.EventHandler {
    private AirViewCamera _airViewCamera;

    [SerializeField] private string _address = "127.0.0.1";
    [SerializeField] private int _port = 9090;

    private void Awake() {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        _airViewCamera = FindObjectOfType<AirViewCamera>();

        AirViewClient.Delegate = this;
    }

    private async void Start() {
        await Task.Yield(); // connects after Start() at which onAirView is loaded.

        _airViewCamera.profile.userID = "sample";

        AirViewClient.Connect(_airViewCamera, _address, _port);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha0)) {
            _airViewCamera.SetAspectRatio(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1)) {
            _airViewCamera.SetAspectRatio(16.0f / 9.0f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            _airViewCamera.SetAspectRatio(1.0f);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            _airViewCamera.SetAspectRatio(1.0f / 2.0f);
        }

        if (Input.GetKeyDown(KeyCode.A)) {
            _airViewCamera.SetProjection(30.0f);
        }
        else if (Input.GetKeyDown(KeyCode.S)) {
            _airViewCamera.SetProjection(60.0f);
        }
        else if (Input.GetKeyDown(KeyCode.D)) {
            _airViewCamera.SetProjection(90.0f);
        }
    }

    // implements AirViewClient.EventHandler
    void AirViewClient.EventHandler.AirViewClientFailed(string reason) {
        Debug.LogWarningFormat("[onAirView] client failed : {0}", reason);
    }

    void AirViewClient.EventHandler.AirViewClientConnected() {
        AirViewClient.Play();
    }

    void AirViewClient.EventHandler.AirViewClientPlaybackStarted() {
        Debug.Log("[onAirView] playback started");
    }

    void AirViewClient.EventHandler.AirViewClientPlaybackStopped() {
        Debug.Log("[onAirView] playback stopped");
    }

    void AirViewClient.EventHandler.AirViewClientDisconnected() {
        Debug.Log("[onAirView] client disconnected");
    }
}
 */