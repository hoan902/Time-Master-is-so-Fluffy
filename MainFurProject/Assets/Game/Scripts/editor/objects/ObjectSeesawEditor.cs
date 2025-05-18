using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectSeesaw))]
public class ObjectSeesawEditor : Editor
{
    private SerializedProperty m_size;
    void OnEnable()
    {
        m_size = serializedObject.FindProperty("m_size");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_size);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            (target as ObjectSeesaw).UpdateSize();
        }
    }
}
