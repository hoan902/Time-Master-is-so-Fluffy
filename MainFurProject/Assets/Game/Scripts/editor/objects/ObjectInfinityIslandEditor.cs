using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(ObjectInfinityIsland))]
public class ObjectInfinityIslandEditor : Editor 
{
    private ObjectInfinityIsland m_infinityIsland;

    private SerializedProperty m_targetStartPoint;
    private SerializedProperty m_targetEndPoint;
    private SerializedProperty m_targetIslandSpeed;
    private SerializedProperty m_targetLocalNodes;
    private SerializedProperty m_targetKey;
    private SerializedProperty m_targetIslandList;
    private SerializedProperty m_targetChildStartIndexs;
    private SerializedProperty m_targetLoop;

    private void OnEnable() 
    {
        m_infinityIsland = target as ObjectInfinityIsland;

        m_targetStartPoint = serializedObject.FindProperty(nameof(m_infinityIsland.startPoint));
        m_targetEndPoint = serializedObject.FindProperty(nameof(m_infinityIsland.endPoint));
        m_targetIslandSpeed = serializedObject.FindProperty(nameof(m_infinityIsland.islandSpeed));
        m_targetLocalNodes = serializedObject.FindProperty(nameof(m_infinityIsland.localNodes));
        m_targetKey = serializedObject.FindProperty(nameof(m_infinityIsland.key));
        m_targetIslandList = serializedObject.FindProperty(nameof(m_infinityIsland.islands));
        m_targetChildStartIndexs = serializedObject.FindProperty(nameof(m_infinityIsland.childStartIndexs));
        m_targetLoop = serializedObject.FindProperty(nameof(m_infinityIsland.loop));
    }

    public override void OnInspectorGUI() 
    {
        // base.OnInspectorGUI();    
        
        serializedObject.Update();

        // EditorGUILayout.PropertyField(m_targetKey);
        EditorGUILayout.PropertyField(m_targetStartPoint);
        EditorGUILayout.PropertyField(m_targetEndPoint);
        EditorGUILayout.PropertyField(m_targetIslandSpeed);
        EditorGUILayout.PropertyField(m_targetLoop);

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Path", EditorStyles.boldLabel);
        if(GUILayout.Button("Add Node"))
        {
            Undo.RecordObject(target, "added node");

            Vector3 position = m_infinityIsland.localNodes[m_infinityIsland.localNodes.Length - 1] + Vector3.right;

            int index = m_targetLocalNodes.arraySize;
            m_targetLocalNodes.InsertArrayElementAtIndex(index);
            m_targetLocalNodes.GetArrayElementAtIndex(index).vector3Value = position;
        }

        EditorGUIUtility.labelWidth = 64;
        int delete = -1;
        for(int i = 0; i < m_infinityIsland.localNodes.Length; ++i)
        {
            int size = 64;
            EditorGUILayout.BeginVertical(GUILayout.Width(size));
            EditorGUILayout.LabelField("Node " + i, GUILayout.Width(size));

            if (i != 0)
            {
                EditorGUILayout.PropertyField(m_targetLocalNodes.GetArrayElementAtIndex(i), new GUIContent("Local Pos:"), GUILayout.Width(size * 5));
            }                        

            if (i != 0 && GUILayout.Button("Delete Node " + i, GUILayout.Width(size * 3)))
            {
                delete = i;
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUIUtility.labelWidth = 0;

        if(delete != -1)
        {
            m_targetLocalNodes.DeleteArrayElementAtIndex(delete);
        }

        m_infinityIsland.UpdateStartEndPos();

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("Islands", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical();

        EditorGUILayout.IntField("Island Amount: ", m_targetIslandList.arraySize);

        if(GUILayout.Button("Add Island"))
        {
            int index = m_targetIslandList.arraySize;
            m_targetIslandList.InsertArrayElementAtIndex(index);
            m_targetChildStartIndexs.InsertArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            
            m_infinityIsland.AddIsland();
            m_infinityIsland.UpdateAllIslandPos();
        }
        
        if(GUILayout.Button("Remove Island"))
        {
            m_infinityIsland.RemoveIsland();
            
            int index = m_targetIslandList.arraySize - 1;
            m_targetIslandList.DeleteArrayElementAtIndex(index);
            m_targetChildStartIndexs.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
            
            m_infinityIsland.UpdateAllIslandPos();
        }
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        if(GUILayout.Button("Remove All Islands"))
        {
            m_infinityIsland.RemoveAllIslands();
            m_targetIslandList.ClearArray();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI() 
    {
        for (int i = 0; i < m_infinityIsland.localNodes.Length; ++i)
        {
            Vector3 worldPos;
            if (Application.isPlaying)
            {
                worldPos = m_infinityIsland.worldNode[i];
            }
            else
            {
                worldPos = m_infinityIsland.transform.TransformPoint(m_infinityIsland.localNodes[i]);
            }


            Vector3 newWorld = worldPos; 
            if(i != 0)
                newWorld = Handles.PositionHandle(worldPos, Quaternion.identity);

            Handles.color = Color.red;

            if (i == 0)
            {
                if (Application.isPlaying)
                {
                    Handles.DrawDottedLine(worldPos, m_infinityIsland.worldNode[m_infinityIsland.worldNode.Length - 1], 10);
                }
                else
                {
                    Handles.DrawDottedLine(worldPos, m_infinityIsland.transform.TransformPoint(m_infinityIsland.localNodes[m_infinityIsland.localNodes.Length - 1]), 10);
                }
            }
            else
            {
                if (Application.isPlaying)
                {
                    Handles.DrawDottedLine(worldPos, m_infinityIsland.worldNode[i - 1], 10);
                }
                else
                {
                    Handles.DrawDottedLine(worldPos, m_infinityIsland.transform.TransformPoint(m_infinityIsland.localNodes[i - 1]), 10);
                }

                if (worldPos != newWorld)
                {
                    Undo.RecordObject(target, "moved point");
                    
                    m_targetLocalNodes.GetArrayElementAtIndex(i).vector3Value = m_infinityIsland.transform.InverseTransformPoint(newWorld);
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}
