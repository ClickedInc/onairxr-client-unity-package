/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEngine;
using onAirXR.Client;

public class AirVROVRController : MonoBehaviour {
    private OVRControllerHelper _controllerModel;

    private void Awake() {
        _controllerModel = transform.Find("OVRControllerPrefab").GetComponent<OVRControllerHelper>();
    }

    private void Update() {
        var active = !AirVRClient.connected && AirVROVRInputHelper.IsConnected(_controllerModel.m_controller);

        if (_controllerModel.gameObject.activeSelf != active) {
            _controllerModel.gameObject.SetActive(active);
        }
    }
}
