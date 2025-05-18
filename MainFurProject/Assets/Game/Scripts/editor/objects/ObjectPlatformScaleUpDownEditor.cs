using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectPlatformScaleUpDown))]
public class ObjectPlatformScaleUpDownEditor : Editor
{
    private ObjectPlatformScaleUpDown m_platform;

    private SerializedProperty m_blocks;
    private SerializedProperty m_destination;
    private SerializedProperty m_firstScaleDelay;
    private SerializedProperty m_scaleDuration;
    private SerializedProperty m_scaleTime;
    private SerializedProperty m_moveTime;
    private SerializedProperty m_firstScaleUp;

    private void OnEnable()
    {
        m_platform = target as ObjectPlatformScaleUpDown;

        m_blocks = serializedObject.FindProperty(nameof(m_platform.blocks));
        m_destination = serializedObject.FindProperty(nameof(m_platform.endPosition));
        m_firstScaleDelay = serializedObject.FindProperty("m_firstScaleDelay");
        m_scaleDuration = serializedObject.FindProperty("m_scaleDuration");
        m_scaleTime = serializedObject.FindProperty("m_scaleTime");
        m_moveTime = serializedObject.FindProperty("m_moveTime");
        m_firstScaleUp = serializedObject.FindProperty("m_firstScaleUp");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextArea(m_blocks.arraySize.ToString());
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.PropertyField(m_firstScaleUp);
        EditorGUILayout.PropertyField(m_firstScaleDelay);
        EditorGUILayout.PropertyField(m_scaleDuration);
        EditorGUILayout.PropertyField(m_scaleTime);
        EditorGUILayout.PropertyField(m_moveTime);
        EditorGUILayout.PropertyField(m_destination);

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Add Block"))
        {
            int index = m_blocks.arraySize;
            m_blocks.InsertArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();

            m_platform.AddBlock();
            m_platform.UpdateAllBlocksPosition();
            m_platform.UpdateColliderSize();
        }
        if (GUILayout.Button("Remove Block"))
        {
            m_platform.RemoveBlock();

            int index = m_blocks.arraySize - 1;
            m_blocks.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();

            m_platform.UpdateAllBlocksPosition();
            m_platform.UpdateColliderSize();
        }
        if (GUILayout.Button("Remove All"))
        {
            m_platform.RemoveAllBlocks();
            m_blocks.ClearArray();
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void OnSceneGUI()
    {
        Vector3 worldPos = m_platform.transform.TransformPoint(m_platform.endPosition);
        Vector3 newWorld = Handles.PositionHandle(worldPos, Quaternion.identity);

        if (worldPos != newWorld)
        {
            Undo.RecordObject(target, "Move Destination");
            m_platform.endPosition = m_platform.transform.InverseTransformPoint(newWorld);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

