
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectSaw))]
public class ObjectSawEditor : Editor
{
    private SerializedProperty m_path;
    private SerializedProperty m_moveTime;
    private SerializedProperty m_healthReduce;

    void OnEnable()
    {
        m_path = serializedObject.FindProperty("m_path");
        m_moveTime = serializedObject.FindProperty("m_moveTime");
        m_healthReduce = serializedObject.FindProperty("m_healthReduce");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_moveTime);
        EditorGUILayout.PropertyField(m_healthReduce);
        EditorGUILayout.PropertyField(m_path);
        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        ObjectSaw t = (target as ObjectSaw);
        Vector3[] path = t.GetPath();
        Handles.color = Color.red; 
        Handles.DrawPolyLine(path);
        for(int i = 0; i < path.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector2 pos = Handles.PositionHandle(path[i], Quaternion.identity);
            if(EditorGUI.EndChangeCheck())
            {                
                Undo.RecordObject(t, "change path");
                t.UpdatePath(pos, i);                
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}