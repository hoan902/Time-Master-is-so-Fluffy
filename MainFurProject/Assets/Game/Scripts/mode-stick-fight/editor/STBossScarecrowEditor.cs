using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(STBossScarecrow))]
public class STBossScarecrowEditor : Editor
{
    private SerializedProperty m_moveSpeed;
    private SerializedProperty m_scytheDamage;
    private SerializedProperty m_bodyDamage;
    private SerializedProperty m_maxMonsterSpawn;
    private SerializedProperty m_idleTime;
    private SerializedProperty m_coin;
    private SerializedProperty m_deadKeyTrigger;
    private SerializedProperty m_maxHP;

    private void OnEnable() 
    {
        m_moveSpeed = serializedObject.FindProperty("m_moveSpeed");
        m_scytheDamage = serializedObject.FindProperty("m_scytheDamage");
        m_bodyDamage = serializedObject.FindProperty("m_bodyDamage");
        m_maxMonsterSpawn = serializedObject.FindProperty("m_maxMonsterSpawn");
        m_idleTime = serializedObject.FindProperty("m_idleTime");
        m_coin = serializedObject.FindProperty("coin");
        m_deadKeyTrigger = serializedObject.FindProperty("deadKeyTrigger");
        m_maxHP = serializedObject.FindProperty("maxHP");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_deadKeyTrigger);
        EditorGUILayout.PropertyField(m_maxHP);
        EditorGUILayout.PropertyField(m_coin);
        EditorGUILayout.PropertyField(m_moveSpeed);
        EditorGUILayout.PropertyField(m_scytheDamage);
        EditorGUILayout.PropertyField(m_bodyDamage);
        EditorGUILayout.PropertyField(m_maxMonsterSpawn);
        EditorGUILayout.PropertyField(m_idleTime);

        serializedObject.ApplyModifiedProperties();
    }
}
