using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectCar))]
public class ObjectCarEditor : Editor
{
    private SerializedProperty m_speed;
    private SerializedProperty m_startPoint;
    private SerializedProperty m_endPoint;

    private Vector2 m_start;
    private Vector2 m_end;

    void OnEnable()
    {
        m_speed = serializedObject.FindProperty("m_speed");
        m_startPoint = serializedObject.FindProperty("m_startPoint");
        m_endPoint = serializedObject.FindProperty("m_endPoint");
        //
        ObjectCar t = (target as ObjectCar);
        if(t == null)
            return;  
        t.InitPoint();
        m_start = t.GetStartPoint();
        m_end = t.GetEndPoint();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_speed);
        EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_startPoint);
            EditorGUILayout.PropertyField(m_endPoint);
            serializedObject.ApplyModifiedProperties();
        if(EditorGUI.EndChangeCheck())
        {
            UpdateLine();
        }
        serializedObject.ApplyModifiedProperties();
    }
    void OnSceneGUI()
    {
        ObjectCar t = (target as ObjectCar);
        if(t == null)
            return;  
        Handles.color = Color.red;
        EditorGUI.BeginChangeCheck();
            m_start = Handles.PositionHandle(m_start, Quaternion.identity);
        if(EditorGUI.EndChangeCheck())
        {  
            Undo.RecordObject(t, "change path");
            t.UpdatePos(m_start, m_end);                
        }
        EditorGUI.BeginChangeCheck();
            m_end = Handles.PositionHandle(m_end, Quaternion.identity);        
        if(EditorGUI.EndChangeCheck())
        {              
            Undo.RecordObject(t, "change path");
            t.UpdatePos(m_start, m_end);                
        }      
        serializedObject.ApplyModifiedProperties();
    }

    void UpdateLine()
    {
        ObjectCar t = (target as ObjectCar);
        if(t == null)
            return;  
        t.UpdateLine(); 
        m_start = t.GetStartPoint();
        m_end = t.GetEndPoint();
    }
}
