using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectWeather))]
public class ObjectWeatherEditor : Editor
{
    private SerializedProperty m_radius;
    private SerializedProperty m_rate;
    private SerializedProperty m_weather;

    private void OnEnable() 
    {
        m_radius = serializedObject.FindProperty("m_radius");
        m_rate = serializedObject.FindProperty("m_rate");
        m_weather = serializedObject.FindProperty("m_weather");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_weather);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_radius);
        serializedObject.ApplyModifiedProperties();
        if(EditorGUI.EndChangeCheck())
            UpdateSize();
        EditorGUILayout.PropertyField(m_rate);
        serializedObject.ApplyModifiedProperties();
        if(EditorGUI.EndChangeCheck())
            UpdateRate();
        serializedObject.ApplyModifiedProperties();
    }

    void UpdateSize()
    {
        var t = (target as ObjectWeather);
        t.UpdateSize();
    }
    void UpdateRate()
    {
        var t = (target as ObjectWeather);
        t.UpdateRate();
    }
}
