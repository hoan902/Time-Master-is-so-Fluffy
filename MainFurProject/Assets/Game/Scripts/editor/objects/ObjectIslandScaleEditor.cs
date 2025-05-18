using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectIslandScale))]
public class ObjectIslandScaleEditor : Editor
{
    private ObjectIslandScale m_islandScale;

    private SerializedProperty m_leftSize;
    private SerializedProperty m_rightSize;
    private SerializedProperty m_moveSpeed;
    private SerializedProperty m_revertSpeed;

    private void OnEnable() 
    {
        m_islandScale = target as ObjectIslandScale;    

        m_leftSize = serializedObject.FindProperty("m_leftSize");
        m_rightSize = serializedObject.FindProperty("m_rightSize");
        m_moveSpeed = serializedObject.FindProperty("m_moveSpeed");
        m_revertSpeed = serializedObject.FindProperty("m_revertSpeed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_moveSpeed);
        EditorGUILayout.PropertyField(m_revertSpeed);
        serializedObject.ApplyModifiedProperties();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_leftSize);
        EditorGUILayout.PropertyField(m_rightSize);
        serializedObject.ApplyModifiedProperties();
        if(EditorGUI.EndChangeCheck())
        {
            UpdateFloorSize();
        }
        if(GUILayout.Button("Refresh"))
        {
            UpdateRope();
            EditorUtility.SetDirty(m_islandScale.gameObject);
        }
    }

    void UpdateFloorSize()
    {
        m_islandScale.UpdateLeftSize();
        m_islandScale.UpdateRightSize();
    }
    void UpdateRope()
    {
        m_islandScale.UpdateRopePosition();
        m_islandScale.UpdateRopeLenght();
        m_islandScale.UpdatePulleyPosition();
    }
}
