
using UnityEditor;

[CustomEditor(typeof(ObjectFlax))]
public class ObjectFlaxEditor : Editor
{
    private SerializedProperty m_width;
    private SerializedProperty m_healthReduce;
    private SerializedProperty m_forceMagnitude;
    private SerializedProperty m_instantKill;

    void OnEnable()
    {
        m_width = serializedObject.FindProperty("m_width");
        m_healthReduce = serializedObject.FindProperty("m_healthReduce");
        m_forceMagnitude = serializedObject.FindProperty("m_forceMagnitude");
        m_instantKill = serializedObject.FindProperty("m_instantKill");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_healthReduce);
        EditorGUILayout.PropertyField(m_instantKill);
        serializedObject.ApplyModifiedProperties();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_forceMagnitude);
        serializedObject.ApplyModifiedProperties();
        if(EditorGUI.EndChangeCheck())
            UpdateForce();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_width);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            ResizeWidth();
    }

    void ResizeWidth()
    {
        var t = (target as ObjectFlax);
        t.ResizeWidth();
    }

    void UpdateForce()
    {
        var t = (target as ObjectFlax);
        t.UpdateForce();
    }
}
