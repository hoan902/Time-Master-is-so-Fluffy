using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectWater))]
public class ObjectWaterEditor : Editor
{
    private ObjectWater m_water;

    private SerializedProperty m_size;

    private void OnEnable()
    {
        m_water = target as ObjectWater;

        m_size = serializedObject.FindProperty("m_size");  
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        Vector2 newSizeValue = EditorGUILayout.Vector2Field("Size", m_size.vector2Value);
        if (newSizeValue.x > 0f && newSizeValue.y > 0f)
            m_size.vector2Value = newSizeValue;

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            m_water.AdjustComponentSizes();
            EditorUtility.SetDirty(target);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
