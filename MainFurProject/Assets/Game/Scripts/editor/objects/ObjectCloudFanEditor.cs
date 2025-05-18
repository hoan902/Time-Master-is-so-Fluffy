using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectCloudFan))]
public class ObjectCloudFanEditor : Editor
{
    private ObjectCloudFan m_target;

    private SerializedProperty m_height;
    private SerializedProperty m_timeMove;

    void OnEnable()
    {
        m_target = target as ObjectCloudFan;
        m_height = serializedObject.FindProperty("m_height");
        m_timeMove = serializedObject.FindProperty("m_timeMove");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_height);
        EditorGUILayout.PropertyField(m_timeMove);
        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        Handles.color = Color.red;
        Vector2 basePos = m_target.cloud.transform.position;
        Vector2 size = m_target.boxColidder.size;
        Vector2 pos1 = new Vector2(basePos.x - size.x/2, basePos.y);
        Vector2 pos2 = new Vector2(pos1.x, pos1.y + m_height.floatValue);
        Vector2 pos3 = new Vector2(basePos.x + size.x/2, pos2.y);
        Vector3 pos4 = new Vector2(pos3.x, pos1.y);
        Handles.DrawPolyLine(pos1, pos2, pos3, pos4);
    }
}
