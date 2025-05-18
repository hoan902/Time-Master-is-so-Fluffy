using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectElevatorArrow))]
public class ObjectElevatorArrowEditor : Editor
{
    private ObjectElevatorArrow m_elevator;

    private SerializedProperty m_key;
    private SerializedProperty m_sizeX;
    private SerializedProperty m_slowSpeedRatio;

    private void OnEnable()
    {
        m_elevator = target as ObjectElevatorArrow;

        m_key = serializedObject.FindProperty("m_key");
        m_sizeX = serializedObject.FindProperty("m_sizeX");
        m_slowSpeedRatio = serializedObject.FindProperty("m_slowSpeedRatio");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_key);
        EditorGUILayout.PropertyField(m_slowSpeedRatio);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_sizeX);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            UpdatePlatformSize();

        serializedObject.ApplyModifiedProperties();
    }

    void UpdatePlatformSize()
    {
        m_elevator.UpdateBox();
    }
}
