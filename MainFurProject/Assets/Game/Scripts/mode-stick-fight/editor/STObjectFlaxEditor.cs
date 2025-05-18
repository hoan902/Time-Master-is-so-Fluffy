using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(STObjectFlax))]
public class STObjectFlaxEditor : Editor
{
    private SerializedProperty m_width;
    private SerializedProperty m_playerDamage;
    private SerializedProperty m_monsterDamage;
    private SerializedProperty m_forceMagnitude;
    private SerializedProperty m_instantKill;

    void OnEnable()
    {
        m_width = serializedObject.FindProperty("m_width");
        m_playerDamage = serializedObject.FindProperty("m_playerDamage");
        m_monsterDamage = serializedObject.FindProperty("m_monsterDamage");
        m_forceMagnitude = serializedObject.FindProperty("m_forceMagnitude");
        m_instantKill = serializedObject.FindProperty("m_instantKill");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_playerDamage);
        EditorGUILayout.PropertyField(m_monsterDamage);
        // EditorGUILayout.PropertyField(m_instantKill);
        serializedObject.ApplyModifiedProperties();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_forceMagnitude);
        serializedObject.ApplyModifiedProperties();
        if(EditorGUI.EndChangeCheck())
            UpdateForce();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_width);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            ResizeWidth();
    }

    void ResizeWidth()
    {
        var t = (target as STObjectFlax);
        t.ResizeWidth();
    }

    void UpdateForce()
    {
        var t = (target as STObjectFlax);
        t.UpdateForce();
    }
}
