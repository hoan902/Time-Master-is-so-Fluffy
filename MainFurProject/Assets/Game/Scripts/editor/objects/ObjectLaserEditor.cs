using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectLaser))]
public class ObjectLaserEditor : Editor
{
    private SerializedProperty m_timeRotate;
    private SerializedProperty m_useLimits;
    private SerializedProperty m_minAngle;
    private SerializedProperty m_maxAngle;
    private SerializedProperty m_clockwise;
    private SerializedProperty m_lockRotate;

    void OnEnable()
    {
        m_timeRotate = serializedObject.FindProperty("m_timeRotate");
        m_useLimits = serializedObject.FindProperty("m_useLimits");
        m_minAngle = serializedObject.FindProperty("m_minAngle");
        m_maxAngle = serializedObject.FindProperty("m_maxAngle");
        m_clockwise = serializedObject.FindProperty("m_clockwise");
        m_lockRotate = serializedObject.FindProperty("m_lockRotate");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_timeRotate);
        EditorGUILayout.PropertyField(m_useLimits);
        EditorGUILayout.PropertyField(m_lockRotate);
        if(m_useLimits.boolValue)
        {
            EditorGUILayout.PropertyField(m_minAngle);
            EditorGUILayout.PropertyField(m_maxAngle);
        }else
            EditorGUILayout.PropertyField(m_clockwise);
        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        if(!m_useLimits.boolValue)
            return;
        Handles.color = Color.red;
        serializedObject.Update();
        m_minAngle.vector2Value = Handles.PositionHandle(m_minAngle.vector2Value, Quaternion.identity);
        m_maxAngle.vector2Value = Handles.PositionHandle(m_maxAngle.vector2Value, Quaternion.identity);
        serializedObject.ApplyModifiedProperties();
    }
}
