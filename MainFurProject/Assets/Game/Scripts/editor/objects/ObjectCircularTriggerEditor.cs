using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectCircularMoveTrigger))]
public class ObjectCircularTriggerEditor : Editor
{
    private ObjectCircularMoveTrigger m_parent;

    private SerializedProperty m_startPosition;
    private SerializedProperty m_speed;
    private SerializedProperty m_clockwise;
    private SerializedProperty m_centerOffset;
    private SerializedProperty m_childUseMyParams;
    private SerializedProperty m_childs;

    private void OnEnable() 
    {
        m_parent = target as ObjectCircularMoveTrigger;

        m_startPosition = serializedObject.FindProperty("startPos");
        m_speed = serializedObject.FindProperty("m_speed");
        m_clockwise = serializedObject.FindProperty("m_clockwise");
        m_centerOffset = serializedObject.FindProperty("m_centerOffset");
        m_childUseMyParams = serializedObject.FindProperty("m_childUseMyParams");
        m_childs = serializedObject.FindProperty("m_childs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_childUseMyParams);
        EditorGUILayout.PropertyField(m_clockwise);
        EditorGUILayout.PropertyField(m_speed);
        EditorGUILayout.PropertyField(m_centerOffset);
        EditorGUILayout.PropertyField(m_startPosition);
        EditorGUILayout.PropertyField(m_childs);
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        EditorGUI.BeginChangeCheck();
        Vector2 pos2 = Handles.PositionHandle(m_parent.startPos, Quaternion.identity);
        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(m_parent, "change destination");
            m_parent.startPos = pos2;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
