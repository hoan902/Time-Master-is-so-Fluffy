using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(STObjectTriggerVirtualCamera))]
public class STObjectTriggerVirtualCameraEditor : Editor
{
    private SerializedProperty m_key;
    private SerializedProperty m_blendTime;
    private SerializedProperty m_showTime;
    private SerializedProperty m_virtualCameraPosition;

    private void OnEnable() 
    {
        m_key = serializedObject.FindProperty("m_key");
        m_blendTime = serializedObject.FindProperty("m_blendTime");
        m_showTime = serializedObject.FindProperty("m_showTime");
        m_virtualCameraPosition = serializedObject.FindProperty("m_virtualCameraPosition");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_key);
        EditorGUILayout.PropertyField(m_blendTime);
        EditorGUILayout.PropertyField(m_showTime);
        EditorGUILayout.PropertyField(m_virtualCameraPosition);
        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        STObjectTriggerVirtualCamera t = (target as STObjectTriggerVirtualCamera);
        Vector3 destination = t.GetCamPos();
        EditorGUI.BeginChangeCheck();
        Vector2 pos2 = Handles.PositionHandle(destination, Quaternion.identity);
        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(t, "change virtual camera position");
            t.UpdateCamPos(pos2);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
