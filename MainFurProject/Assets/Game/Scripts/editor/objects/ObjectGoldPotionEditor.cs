using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectGoldPotion))]
public class ObjectGoldPotionEditor : Editor
{
    private SerializedProperty m_showTime;
    private SerializedProperty m_radius;
    private SerializedProperty m_isFree;
    private SerializedProperty m_virtualCam;
    private SerializedProperty m_explodeEff;

    private void OnEnable() 
    {
        m_showTime = serializedObject.FindProperty("m_showTime");
        m_radius = serializedObject.FindProperty("m_radius");
        m_isFree = serializedObject.FindProperty("m_isFree");
        m_explodeEff = serializedObject.FindProperty("m_explodeEff");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_showTime);
        EditorGUILayout.PropertyField(m_explodeEff);
        serializedObject.ApplyModifiedProperties();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_isFree);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            UpdateForm();
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_radius);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            UpdateRadius();
    }

    void UpdateForm()
    {
        var t = (target as ObjectGoldPotion);
        t.UpdateForm();
    }

    void UpdateRadius()
    {
        var t = (target as ObjectGoldPotion);
        t.UpdateRadius();
    }
}
