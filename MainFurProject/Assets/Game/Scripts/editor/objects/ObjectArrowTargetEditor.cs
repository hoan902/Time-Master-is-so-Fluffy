using UnityEditor;

[CustomEditor(typeof(ObjectArrowTarget))]
public class ObjectArrowTargetEditor : Editor
{
    private SerializedProperty m_angle;
    private SerializedProperty m_target;
    private void OnEnable()
    {
        m_angle = serializedObject.FindProperty("m_angle");
        m_target = serializedObject.FindProperty("m_target");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_angle);
        EditorGUILayout.PropertyField(m_target);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            (target as ObjectArrowTarget).UpdateAngle();
            (target as ObjectArrowTarget).UpdateTarget();
        }
    }
}
