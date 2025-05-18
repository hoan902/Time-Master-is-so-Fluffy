using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectThorn))]
public class ObjectThornEditor : Editor
{
    private SerializedProperty m_damage;
    private SerializedProperty m_amount;

    private void OnEnable()
    {
        m_damage = serializedObject.FindProperty("m_damage");
        m_amount = serializedObject.FindProperty("m_amount");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_damage);

        serializedObject.ApplyModifiedProperties();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_amount);
        serializedObject.ApplyModifiedProperties();
        if(EditorGUI.EndChangeCheck())
            UpdateThorn();

        serializedObject.ApplyModifiedProperties();
    }

    void UpdateThorn()
    {
        var t = target as ObjectThorn;
        t.RemoveAllThorn();
        t.SpawnThorn();
    }
}
