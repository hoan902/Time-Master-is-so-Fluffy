using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectWoodCloud))]
public class ObjectWoodCloudEditor : Editor
{
    private ObjectWoodCloud m_cloud;
    private SerializedProperty m_refSizeRenderer;

    private void OnEnable() 
    {
        m_cloud = (ObjectWoodCloud)target;
        m_refSizeRenderer = serializedObject.FindProperty("m_refSizeRenderer");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.PropertyField(m_refSizeRenderer);
        if(GUILayout.Button("Update Size"))
        {
            EditorApplication.delayCall += UpdateSize;            
        }
        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }
    void UpdateSize()
    {
        m_cloud.UpdateSize();
    }
}
