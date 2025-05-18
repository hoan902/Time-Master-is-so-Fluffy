using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(STMovingPlatform))]
public class STMovingPlatformEditor : Editor
{
    private STMovingPlatform m_movingPlatform;

    private SerializedProperty m_PlatformTypeProperty;
    private SerializedProperty m_PlatformSpeedProperty;
    private SerializedProperty m_PlatformNodesProperty;
    private SerializedProperty m_PlatformWaitTimeProperty;
    private SerializedProperty m_PlatformAutoPlay;

    float m_PreviewPosition = 0;

    private void OnEnable()
    {
        m_PreviewPosition = 0;
        m_movingPlatform = target as STMovingPlatform;

        if(!EditorApplication.isPlayingOrWillChangePlaymode)
            STMovingPlatformPreview.CreateNewPreview(m_movingPlatform);

        m_PlatformTypeProperty = serializedObject.FindProperty(nameof(m_movingPlatform.platformType));
        m_PlatformSpeedProperty = serializedObject.FindProperty(nameof(m_movingPlatform.speed));
        m_PlatformNodesProperty = serializedObject.FindProperty(nameof(m_movingPlatform.localNodes));
        m_PlatformWaitTimeProperty = serializedObject.FindProperty(nameof(m_movingPlatform.waitTimes));
        m_PlatformAutoPlay = serializedObject.FindProperty(nameof(m_movingPlatform.autoPlay));
    }

    private void OnDisable()
    {
        STMovingPlatformPreview.DestroyPreview();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_PlatformAutoPlay);

        EditorGUI.BeginChangeCheck();
        m_PreviewPosition = EditorGUILayout.Slider("Preview position", m_PreviewPosition, 0.0f, 1.0f);
        if (EditorGUI.EndChangeCheck())
        {
            MovePreview();
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(m_PlatformTypeProperty);
        EditorGUILayout.PropertyField(m_PlatformSpeedProperty);

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        if (GUILayout.Button("Add Node"))
        {
            Undo.RecordObject(target, "added node");


            Vector3 position = m_movingPlatform.localNodes[m_movingPlatform.localNodes.Length - 1] + Vector2.right;

            int index = m_PlatformNodesProperty.arraySize;
            m_PlatformNodesProperty.InsertArrayElementAtIndex(index);
            m_PlatformNodesProperty.GetArrayElementAtIndex(index).vector2Value = position;

            m_PlatformWaitTimeProperty.InsertArrayElementAtIndex(index);
            m_PlatformWaitTimeProperty.GetArrayElementAtIndex(index).floatValue = 0;
        }

        EditorGUIUtility.labelWidth = 64;
        int delete = -1;
        for (int i = 0; i < m_movingPlatform.localNodes.Length; ++i)
        {
            //EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();

            int size = 64;
            EditorGUILayout.BeginVertical(GUILayout.Width(size));
            EditorGUILayout.LabelField("Node " + i, GUILayout.Width(size));
            if (i != 0 && GUILayout.Button("Delete", GUILayout.Width(size)))
            {
                delete = i;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();

            if (i != 0)
            {
                EditorGUILayout.PropertyField(m_PlatformNodesProperty.GetArrayElementAtIndex(i), new GUIContent("Pos"));
                //EditorGUILayout.PropertyField(m_PlatformWaitTimeProperty.GetArrayElementAtIndex(i), new GUIContent("Wait Time"));
            }

            EditorGUILayout.PropertyField(m_PlatformWaitTimeProperty.GetArrayElementAtIndex(i),
                new GUIContent("Wait Time"));

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

        }

        EditorGUIUtility.labelWidth = 0;

        if (delete != -1)
        {
            m_PlatformNodesProperty.DeleteArrayElementAtIndex(delete);
            m_PlatformWaitTimeProperty.DeleteArrayElementAtIndex(delete);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        if (Application.isPlaying)
            return;
        MovePreview();

        for (int i = 0; i < m_movingPlatform.localNodes.Length; ++i)
        {
            Vector2 worldPos = m_movingPlatform.transform.TransformPoint(m_movingPlatform.localNodes[i]);

            Vector2 newWorld = worldPos;
            if (i != 0)
                newWorld = Handles.PositionHandle(worldPos, Quaternion.identity);

            Handles.color = Color.red;

            if (i != 0)
            {
                Handles.DrawDottedLine(worldPos, m_movingPlatform.transform.TransformPoint(m_movingPlatform.localNodes[i - 1]), 10);
                if (worldPos != newWorld)
                {
                    Undo.RecordObject(target, "moved point");

                    m_PlatformNodesProperty.GetArrayElementAtIndex(i).vector2Value =
                        m_movingPlatform.transform.InverseTransformPoint(newWorld);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }

    void MovePreview()
    {
        //compute pos from 0-1 preview pos
        if (Application.isPlaying)
            return;
        float step = 1.0f / (m_movingPlatform.localNodes.Length - 1);
        int starting = Mathf.FloorToInt(m_PreviewPosition / step);
        if (starting > m_movingPlatform.localNodes.Length - 2)
            return;
        float localRatio = (m_PreviewPosition - (step * starting)) / step;
        Vector3 localPos = Vector3.Lerp(m_movingPlatform.localNodes[starting],
            m_movingPlatform.localNodes[starting + 1], localRatio);
        STMovingPlatformPreview.preview.transform.position = m_movingPlatform.transform.TransformPoint(localPos);
        SceneView.RepaintAll();
    }
}
