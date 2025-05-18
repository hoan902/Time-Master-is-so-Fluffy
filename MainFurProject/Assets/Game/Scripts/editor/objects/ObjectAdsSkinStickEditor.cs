using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectAdsSkinStick))]
public class ObjectAdsSkinStickEditor : Editor
{
    private SerializedProperty m_skinID;
    private SerializedProperty m_isFree;

    void OnEnable()
    {
        m_skinID = serializedObject.FindProperty("m_skinID");
        m_isFree = serializedObject.FindProperty("m_isFree");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_skinID);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            UpdateSkin();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_isFree);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            UpdateForm();
    }

    void UpdateSkin()
    {
        var t = (target as ObjectAdsSkinStick);
        t.UpdateSkin();
    }

    void UpdateForm()
    {
        var t = (target as ObjectAdsSkinStick);
        t.UpdateForm();
    }
}
