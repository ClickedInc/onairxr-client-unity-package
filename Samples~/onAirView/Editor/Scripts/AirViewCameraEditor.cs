/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AirViewCamera))]
public class AirViewCameraEditor : Editor {
    private SerializedProperty _propAnchor;
    private SerializedProperty _propSendScreenTouches;
    private SerializedProperty _propForceStereoscopicInEditor;
    private SerializedProperty _propVideoCodecInEditor;

    private void OnEnable() {
        _propAnchor = serializedObject.FindProperty("_anchor");
        _propSendScreenTouches = serializedObject.FindProperty("_sendScreenTouches");
        _propForceStereoscopicInEditor = serializedObject.FindProperty("_forceStereoscopicInEditor");
        _propVideoCodecInEditor = serializedObject.FindProperty("_videoCodecInEditor");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_propAnchor, Styles.labelAnchor);
        EditorGUILayout.PropertyField(_propSendScreenTouches, Styles.labelSendScreenTouches);

        EditorGUILayout.BeginVertical("Box");
        {
            EditorGUILayout.LabelField("Editor Only", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_propForceStereoscopicInEditor, Styles.labelForceStereoscopicInEditor);
            EditorGUILayout.PropertyField(_propVideoCodecInEditor, Styles.labelVideoCodecInEditor);
        }
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    private bool hasCamera() {
        return (serializedObject.targetObject as AirViewCamera).GetComponent<Camera>() != null;
    }

    private class Styles {
        public static GUIStyle fontNote = new GUIStyle(EditorStyles.wordWrappedLabel);

        public static GUIContent labelAnchor = new GUIContent("Anchor");
        public static GUIContent labelSendScreenTouches = new GUIContent("Send Screen Touches");
        public static GUIContent labelForceStereoscopicInEditor = new GUIContent("Stereoscopic");
        public static GUIContent labelVideoCodecInEditor = new GUIContent("Video Codec");

        static Styles() {
            fontNote.fontStyle = FontStyle.Italic;
        }
    }
}
