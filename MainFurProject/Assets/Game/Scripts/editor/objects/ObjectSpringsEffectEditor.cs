using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectSpringsEffect))]
public class ObjectSpringsEffectEditor : Editor
{
    [SerializeField] private SerializedProperty m_force;
    [SerializeField] private SerializedProperty m_size;

    private void OnEnable()
    {
        m_force = serializedObject.FindProperty("m_force");
        m_size = serializedObject.FindProperty("m_size");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_force);

        serializedObject.ApplyModifiedProperties();

        //EditorGUI.BeginChangeCheck();
        //EditorGUILayout.PropertyField(m_size);
        //serializedObject.ApplyModifiedProperties();
        //if (EditorGUI.EndChangeCheck())
        //    UpdateSize();
    }

    void UpdateSize()
    {
        var t = target as ObjectSpringsEffect;
        t.UpdateSize();
    }
}
