using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectSeeSawLight))]
public class ObjectSeeSawLightEditor : Editor
{
    private SerializedProperty m_size;
    private SerializedProperty m_lightInner;
    private SerializedProperty m_lightOutter;

    private void OnEnable() 
    {
        m_size = serializedObject.FindProperty("m_size");
        m_lightInner = serializedObject.FindProperty("m_lightInner");
        m_lightOutter = serializedObject.FindProperty("m_lightOutter");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_size);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            (target as ObjectSeeSawLight).UpdateSize();
        }

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_lightInner);
        EditorGUILayout.PropertyField(m_lightOutter);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            (target as ObjectSeeSawLight).UpdateLightSize();
        }

    }
}
