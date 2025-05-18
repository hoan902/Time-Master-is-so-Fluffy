using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectArrow))]
public class ObjectArrowEditor : Editor
{
    private SerializedProperty m_angle;
    private void OnEnable()
    {
        m_angle = serializedObject.FindProperty("m_angle");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_angle);
        serializedObject.ApplyModifiedProperties();
        if(EditorGUI.EndChangeCheck())
        {
            (target as ObjectArrow).UpdateAngle();
        }
    }
}
