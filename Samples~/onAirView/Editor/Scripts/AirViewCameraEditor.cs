/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AirViewCamera))]
public class AirViewCameraEditor : Editor {
    private SerializedProperty _propSendScreenTouches;
    private SerializedProperty _propVideoBitrate;
    private SerializedProperty _propTargetTexture;
    private SerializedProperty _propManualFollowTransform;
    private SerializedProperty _propManualFieldOfView;
    private SerializedProperty _propManualAspectRatio;
    private SerializedProperty _propForceStereoscopicInEditor;
    private SerializedProperty _propVideoCodecInEditor;

    private void OnEnable() {
        _propVideoBitrate = serializedObject.FindProperty("_videoBitrate");
        _propSendScreenTouches = serializedObject.FindProperty("_sendScreenTouches");
        _propTargetTexture = serializedObject.FindProperty("_targetTexture");
        _propManualFollowTransform = serializedObject.FindProperty("_manualFollowTransform");
        _propManualFieldOfView = serializedObject.FindProperty("_manualFieldOfView");
        _propManualAspectRatio = serializedObject.FindProperty("_manualAspectRatio");
        _propForceStereoscopicInEditor = serializedObject.FindProperty("_forceStereoscopicInEditor");
        _propVideoCodecInEditor = serializedObject.FindProperty("_videoCodecInEditor");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_propVideoBitrate, Styles.labelVideoBitrate);
        EditorGUILayout.PropertyField(_propSendScreenTouches, Styles.labelSendScreenTouches);

        EditorGUILayout.BeginVertical("Box");
        if (hasCamera()) {
            EditorGUILayout.LabelField(Styles.textHasCamera, Styles.fontNote);
        }
        else {
            EditorGUILayout.LabelField(Styles.titleCameraSettings, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_propManualFollowTransform, Styles.labelFollowTransform);
            EditorGUILayout.PropertyField(_propManualFieldOfView, Styles.labelFieldOfView);
            EditorGUILayout.PropertyField(_propManualAspectRatio, Styles.labelAspectRatio);
            EditorGUILayout.PropertyField(_propTargetTexture, Styles.labelTargetTexture);
        }
        EditorGUILayout.EndVertical();

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

        public static GUIContent labelVideoBitrate = new GUIContent("Video Quality");
        public static GUIContent labelSendScreenTouches = new GUIContent("Send Screen Touches");
        public static GUIContent labelForceStereoscopicInEditor = new GUIContent("Stereoscopic");
        public static GUIContent labelVideoCodecInEditor = new GUIContent("Video Codec");

        public static GUIContent textHasCamera = new GUIContent("All camera settings follow the attached camera.");
        public static GUIContent titleCameraSettings = new GUIContent("Camera Settings");
        public static GUIContent labelFollowTransform = new GUIContent("Follow Transform");
        public static GUIContent labelFieldOfView = new GUIContent("Field of View");
        public static GUIContent labelAspectRatio = new GUIContent("Aspect Ratio");
        public static GUIContent labelTargetTexture = new GUIContent("Target Texture");

        static Styles() {
            fontNote.fontStyle = FontStyle.Italic;
        }
    }
}
