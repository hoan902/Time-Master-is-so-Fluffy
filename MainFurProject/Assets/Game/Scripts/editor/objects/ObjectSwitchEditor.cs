using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectSwitch))]
public class ObjectSwitchEditor : Editor
{
    private SerializedProperty m_keys;
    private SerializedProperty m_syncKey;
    private SerializedProperty m_invert;

    void OnEnable()
    {
        m_keys = serializedObject.FindProperty("m_keys");
        m_syncKey = serializedObject.FindProperty("m_syncKey");
        m_invert = serializedObject.FindProperty("m_invert");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_keys);
        EditorGUILayout.PropertyField(m_syncKey);
        serializedObject.ApplyModifiedProperties();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_invert);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            Invert();
    }

    void Invert()
    {
        ObjectSwitch t = target as ObjectSwitch;   
        t.InitDirection();
    }
}
