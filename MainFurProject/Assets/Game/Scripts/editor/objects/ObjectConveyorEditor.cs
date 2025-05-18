using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectConveyor))]
public class ObjectConveyorEditor : Editor
{
    private SerializedProperty m_speed;
    private SerializedProperty m_startPoint;
    private SerializedProperty m_endPoint;

    void OnEnable()
    {
        m_speed = serializedObject.FindProperty("m_speed");
        m_startPoint = serializedObject.FindProperty("m_startPoint");
        m_endPoint = serializedObject.FindProperty("m_endPoint");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_speed);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_startPoint);
        EditorGUILayout.PropertyField(m_endPoint);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            UpdateLine();
        }
    }

    void OnSceneGUI()
    {
        Handles.color = Color.red;
        EditorGUI.BeginChangeCheck();
        m_startPoint.vector3Value = Handles.PositionHandle((Vector2)m_startPoint.vector3Value, Quaternion.identity);
        m_endPoint.vector3Value = Handles.PositionHandle((Vector2)m_endPoint.vector3Value, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "change path");
            UpdateLine();
        }
        serializedObject.ApplyModifiedProperties();
    }

    void UpdateLine()
    {
        ObjectConveyor t = (target as ObjectConveyor);
        if (t == null)
            return;
        t.UpdateLine();
    }
}
