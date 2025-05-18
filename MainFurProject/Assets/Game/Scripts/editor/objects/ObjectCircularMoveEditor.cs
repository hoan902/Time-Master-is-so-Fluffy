using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectCircularMove))]
public class ObjectCircularMoveEditor : Editor
{
    private ObjectCircularMove m_mover;

    private SerializedProperty m_startPosition;
    private SerializedProperty m_speed;
    private SerializedProperty m_clockwise;
    private SerializedProperty m_centerOffset;
    private SerializedProperty m_localStartPos;

    private void OnEnable() 
    {
        m_mover = target as ObjectCircularMove;

        m_startPosition = serializedObject.FindProperty("startPosition");
        m_speed = serializedObject.FindProperty("m_speed");
        m_clockwise = serializedObject.FindProperty("m_clockwise");
        m_centerOffset = serializedObject.FindProperty("m_centerOffset");
        m_localStartPos = serializedObject.FindProperty("localStartPos");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_localStartPos);
        EditorGUILayout.PropertyField(m_speed);
        EditorGUILayout.PropertyField(m_centerOffset);
        EditorGUILayout.PropertyField(m_clockwise);

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI() 
    {
        Vector3 worldPos;
        if (Application.isPlaying)
        {
            worldPos = m_mover.startPosition;
        }
        else
        {
            worldPos = m_mover.transform.TransformPoint(m_mover.localStartPos);
        }
        Vector3 newWorld = worldPos;
        newWorld = Handles.PositionHandle(worldPos, Quaternion.identity);

        if(worldPos != newWorld)
        {
            Undo.RecordObject(target, "moved point");
                    
            m_mover.localStartPos = m_mover.transform.InverseTransformPoint(newWorld);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
