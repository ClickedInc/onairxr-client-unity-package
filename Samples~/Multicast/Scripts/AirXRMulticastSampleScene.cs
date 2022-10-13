/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Collections.Generic;
using UnityEngine;
using onAirXR.Client;

public class AirXRMulticastSampleScene : MonoBehaviour, AXRMulticastManager.EventListener {
    private Transform _member;

    private Dictionary<byte, Color> _subgroups = new Dictionary<byte, Color>();
    private Dictionary<string, Transform> _others = new Dictionary<string, Transform>();

    [SerializeField] private string _multicastAddress = "239.18.0.1";
    [SerializeField] private int _multicastPort = 1888;

#pragma warning disable 0414
    [SerializeField] private string _multicastNetAddress = "";
    [SerializeField] private string _multicastInterfaceAndroid = "wlan0";
    [SerializeField] private string _multicastInterfaceIOS = "en0";
#pragma warning restore 0414

    [SerializeField] private SubgroupColor[] _subgroupColors = null;

    private void Awake() {
        if (Application.platform == RuntimePlatform.Android) {
            Application.targetFrameRate = 60;
        }

        _member = transform.Find("Member");
        placeToRandomPosition(_member);

        var isIPv6 = _multicastAddress.Split(':').Length > 2;
        if (isIPv6) {
#if !UNITY_EDITOR && UNITY_ANDROID
            AirXRMulticastManager.LoadOnce(_multicastAddress, _multicastPort, _multicastInterfaceAndroid);
#elif !UNITY_EDITOR && UNITY_IOS
            AirXRMulticastManager.LoadOnce(_multicastAddress, _multicastPort, _multicastInterfaceIOS);
#else
            AXRMulticastManager.LoadOnce(_multicastAddress, _multicastPort, _multicastNetAddress);
#endif
        }
        else {
            AXRMulticastManager.LoadOnce(_multicastAddress, _multicastPort, _multicastNetAddress);
        }

        AXRMulticastManager.RegisterDelegate(this);

        if (_subgroupColors != null) {
            foreach (var subgroup in _subgroupColors) {
                _subgroups[subgroup.subgroup] = subgroup.color;
            }
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1) && _subgroups.ContainsKey(1)) {
            AXRMulticastManager.SetSubgroup(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && _subgroups.ContainsKey(2)) {
            AXRMulticastManager.SetSubgroup(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && _subgroups.ContainsKey(3)) {
            AXRMulticastManager.SetSubgroup(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && _subgroups.ContainsKey(4)) {
            AXRMulticastManager.SetSubgroup(4);
        }
        else if (Input.GetKeyDown(KeyCode.J)) {
            AXRMulticastManager.Join();
        }
        else if (Input.GetKeyDown(KeyCode.L)) {
            AXRMulticastManager.Leave();
        }
    }

    // implements AXRMulticastManager.EventListener
    void AXRMulticastManager.EventListener.MemberJoined(AXRMulticastManager manager, string member, byte subgroup) {
        if (_others.ContainsKey(member)) { return; }

        var other = Instantiate(_member.gameObject).transform;
        other.gameObject.name = "Other: " + member;
        other.gameObject.SetActive(true);
        other.parent = transform;
        other.localPosition = Vector3.zero;
        other.localRotation = Quaternion.identity;
        other.GetComponent<Animation>().enabled = false;

        _others.Add(member, other);
    }

    void AXRMulticastManager.EventListener.MemberChangedMembership(AXRMulticastManager manager, string member, byte subgroup) {
        if (_others.ContainsKey(member) == false) { return; }

        _others[member].GetComponent<MeshRenderer>().material.color = _subgroups.ContainsKey(subgroup) ? _subgroups[subgroup] : Color.white;
    }

    void AXRMulticastManager.EventListener.MemberLeft(AXRMulticastManager manager, string member) {
        if (_others.ContainsKey(member) == false) { return; }

        Destroy(_others[member].gameObject);
        _others.Remove(member);
    }

    void AXRMulticastManager.EventListener.GetInputsPerFrame(AXRMulticastManager manager) {
        byte value = 0;
        var position = Vector3.zero;
        var rotation = Quaternion.identity;

        foreach (var member in _others.Keys) {
            if (manager.GetInputByteStream(member, 0, 0, ref value)) {
                Debug.Log(string.Format("{0}: byte stream = {1}", member, value));
            }
            if (manager.GetInputPose(member, 0, 1, ref position, ref rotation)) {
                _others[member].localPosition = position;
                _others[member].localRotation = rotation;
            }
        }
    }

    bool AXRMulticastManager.EventListener.PendInputsPerFrame(AXRMulticastManager manager) {
        manager.PendInputByteStream(0, 0, 23);
        manager.PendInputPose(0, 1, _member.localPosition, _member.localRotation);
        return true;
    }

    private void placeToRandomPosition(Transform xform) {
        xform.localPosition = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.1f, 0.5f), Random.Range(-0.5f, 0.5f));
    }

    [System.Serializable]
    private struct SubgroupColor {
        #pragma warning disable 0649

        public byte subgroup;
        public Color color;

        #pragma warning restore 0649
    }
}
