
using UnityEditor;

[CustomEditor(typeof(ObjectAds))]
public class ObjectAdsEditor : Editor
{
    private SerializedProperty m_spriteType;
    private SerializedProperty m_quantity;
    private SerializedProperty m_isFree;

    void OnEnable()
    {
        m_spriteType = serializedObject.FindProperty("m_spriteType");
        m_quantity = serializedObject.FindProperty("m_quantity");
        m_isFree = serializedObject.FindProperty("m_isFree");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_spriteType);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            UpdateSprite();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_quantity);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            UpdateText();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_isFree);
        serializedObject.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
            UpdateForm();
    }

    void UpdateSprite()
    {
        var t = (target as ObjectAds);
        t.UpdateSprite();
    }

    void UpdateText()
    {
        var t = (target as ObjectAds);
        t.UpdateText();
    }

    void UpdateForm()
    {
        var t = (target as ObjectAds);
        t.UpdateForm();
    }
}
