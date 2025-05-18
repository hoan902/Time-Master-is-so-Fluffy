using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectTeleportMask))]
public class ObjectTeleportMaskEditor : Editor
{
    private SerializedProperty m_destination;
    private SerializedProperty m_hasDestroy;

    private void OnEnable() 
    {
        m_destination = serializedObject.FindProperty("m_destination");
        m_hasDestroy = serializedObject.FindProperty("m_hasDestroy");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_destination);
        EditorGUILayout.PropertyField(m_hasDestroy);
        serializedObject.ApplyModifiedProperties();
    }
    void OnSceneGUI()
    {
        ObjectTeleportMask t = (target as ObjectTeleportMask);
        Vector3 destination = t.GetDestination();
        EditorGUI.BeginChangeCheck();
        Vector2 pos2 = Handles.PositionHandle(destination, Quaternion.identity);
        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(t, "change destination");
            t.UpdateDestination(pos2);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
