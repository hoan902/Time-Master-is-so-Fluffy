
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectAdsPath))]
public class ObjectAdsPathEditor : Editor
{
    private SerializedProperty m_spriteType;
    private SerializedProperty m_quantity;
    private SerializedProperty m_isFree;
    private SerializedProperty m_path;
    private SerializedProperty m_virtualCam;
    private SerializedProperty m_explodeEff;
    private SerializedProperty m_moveTime;
    private SerializedProperty m_destination;

    void OnEnable()
    {
        m_spriteType = serializedObject.FindProperty("m_spriteType");
        m_quantity = serializedObject.FindProperty("m_quantity");
        m_isFree = serializedObject.FindProperty("m_isFree");
        m_path = serializedObject.FindProperty("m_path");
        m_virtualCam = serializedObject.FindProperty("m_virtualCam");
        m_explodeEff = serializedObject.FindProperty("m_explodeEff");
        m_moveTime = serializedObject.FindProperty("m_moveTime");
        m_destination = serializedObject.FindProperty("m_destination");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_virtualCam);
        EditorGUILayout.PropertyField(m_explodeEff);
        EditorGUILayout.PropertyField(m_moveTime);
        EditorGUILayout.PropertyField(m_path);
        EditorGUILayout.PropertyField(m_destination);
        serializedObject.ApplyModifiedProperties();
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

    void OnSceneGUI()
    {
        ObjectAdsPath t = (target as ObjectAdsPath);
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

    void UpdateSprite()
    {
        var t = (target as ObjectAdsPath);
        t.UpdateSprite();
    }

    void UpdateText()
    {
        var t = (target as ObjectAdsPath);
        t.UpdateText();
    }

    void UpdateForm()
    {
        var t = (target as ObjectAdsPath);
        t.UpdateForm();
    }
}
