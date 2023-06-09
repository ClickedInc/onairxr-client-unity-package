/***********************************************************

  Copyright (c) 2017-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AirVRCamera))]

public class AirVRCameraEditor : Editor {
    private SerializedProperty _propAnchor;    
    private SerializedProperty _propAudioMixerGroup;

    private void OnEnable() {
        _propAnchor = serializedObject.FindProperty("_anchor");
        _propAudioMixerGroup = serializedObject.FindProperty("_audioMixerGroup");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_propAnchor);
        EditorGUILayout.PropertyField(_propAudioMixerGroup);

        serializedObject.ApplyModifiedProperties();
    }
}
