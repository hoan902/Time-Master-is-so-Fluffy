using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectAdsWeaponStick))]
public class ObjectAdsWeaponStickEditor : Editor
{
    private SerializedProperty m_weapon;
    private SerializedProperty m_isFree;

    void OnEnable()
    {
        m_weapon = serializedObject.FindProperty("m_weapon");
        m_isFree = serializedObject.FindProperty("m_isFree");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
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
    }

    void UpdateSkin()
    {
        var t = (target as ObjectAdsWeaponStick);
        t.UpdateSkin();
    }

    void UpdateForm()
    {
        var t = (target as ObjectAdsWeaponStick);
        t.UpdateForm();
    }
}
