using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(STObjectSaw))]
public class STObjectSawEditor : Editor
{
    private SerializedProperty m_path;
    private SerializedProperty m_moveTime;
    private SerializedProperty m_playerDamage;
    private SerializedProperty m_monsterDamage;

    void OnEnable()
    {
        m_path = serializedObject.FindProperty("m_path");
        m_moveTime = serializedObject.FindProperty("m_moveTime");
        m_playerDamage = serializedObject.FindProperty("m_playerDamage");
        m_monsterDamage = serializedObject.FindProperty("m_monsterDamage");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_moveTime);
        EditorGUILayout.PropertyField(m_playerDamage);
        EditorGUILayout.PropertyField(m_monsterDamage);
        EditorGUILayout.PropertyField(m_path);
        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        STObjectSaw t = (target as STObjectSaw);
        Vector3[] path = t.GetPath();
        Handles.color = Color.red; 
        Handles.DrawPolyLine(path);
        for(int i = 0; i < path.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector2 pos = Handles.PositionHandle(path[i], Quaternion.identity);
            if(EditorGUI.EndChangeCheck())
            {                
                Undo.RecordObject(t, "change path");
                t.UpdatePath(pos, i);                
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
