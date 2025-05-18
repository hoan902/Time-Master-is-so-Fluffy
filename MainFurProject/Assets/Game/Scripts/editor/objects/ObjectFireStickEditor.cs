using UnityEditor;

[CustomEditor(typeof(ObjectFireStick))]
public class ObjectFireStickEditor : Editor
{
    private SerializedProperty m_lenght;
    private SerializedProperty m_duration;
    private SerializedProperty m_clockwise;

    void OnEnable() 
    {
        m_lenght = serializedObject.FindProperty("m_lenght");
        m_duration = serializedObject.FindProperty("m_duration");
        m_clockwise = serializedObject.FindProperty("m_clockwise");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_duration);
        EditorGUILayout.PropertyField(m_clockwise);
        serializedObject.ApplyModifiedProperties();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_lenght);
        serializedObject.ApplyModifiedProperties();
        if(EditorGUI.EndChangeCheck())
            UpdateSize(m_lenght.floatValue);
    }

    void UpdateSize(float lenght)
    {
        var t = (target as ObjectFireStick);
        t.UpdateSize(lenght);
    }
}
