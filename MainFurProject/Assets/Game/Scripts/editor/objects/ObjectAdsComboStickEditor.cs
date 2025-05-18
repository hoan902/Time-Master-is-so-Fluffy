using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectAdsComboStick))]
public class ObjectAdsComboStickEditor : Editor
{
    private SerializedProperty m_skinID;
    private SerializedProperty m_weapon;
    private SerializedProperty m_isFree;
    private SerializedProperty m_canKeepSkin;

    private void OnEnable()
    {
        m_skinID = serializedObject.FindProperty("m_skinID");
        m_weapon = serializedObject.FindProperty("m_weapon");
        m_isFree = serializedObject.FindProperty("m_isFree");
        m_canKeepSkin = serializedObject.FindProperty("m_canKeepSkin");
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
        EditorGUILayout.PropertyField(m_weapon);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            UpdateSkin();
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_isFree);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            UpdateForm();
        
        EditorGUILayout.PropertyField(m_canKeepSkin);
        serializedObject.ApplyModifiedProperties();
    }
    
    void UpdateSkin()
    {
        var t = (target as ObjectAdsComboStick);
        t.UpdateSkin();
    }

    void UpdateForm()
    {
        var t = (target as ObjectAdsComboStick);
        t.UpdateForm();
    }
}
