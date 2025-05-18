using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

[CustomEditor(typeof(STBossGiantGolem))]
public class STBossGiantGolemEditor : Editor
{
    private STBossGiantGolem m_boss;
    
    private SerializedProperty m_deadKeyTrigger;
    private SerializedProperty m_maxHP;
    private SerializedProperty m_coin;
    private SerializedProperty m_visibleRange;
    private SerializedProperty m_stunTime;
    private SerializedProperty m_handDamage;
    private SerializedProperty m_bulletDamage;
    private SerializedProperty m_bulletSpeed;
    private SerializedProperty m_bigBulletDamage;
    private SerializedProperty m_bigBulletSpeed;
    private SerializedProperty m_leftPos;
    private SerializedProperty m_rightPos;
    private SerializedProperty m_topLeftPos;
    private SerializedProperty m_topRightPos;

    private void OnEnable()
    {
        m_boss = target as STBossGiantGolem;
        
        m_deadKeyTrigger = serializedObject.FindProperty("deadKeyTrigger");
        m_maxHP = serializedObject.FindProperty("maxHP");
        m_coin = serializedObject.FindProperty("coin");
        m_visibleRange = serializedObject.FindProperty("visibleRange");
        m_stunTime = serializedObject.FindProperty("m_stunTime");
        m_handDamage = serializedObject.FindProperty("m_handDamage");
        m_bulletDamage = serializedObject.FindProperty("m_bulletDamge");
        m_bulletSpeed = serializedObject.FindProperty("m_bulletSpeed");
        m_bigBulletDamage = serializedObject.FindProperty("m_bigBulletDamage");
        m_bigBulletSpeed = serializedObject.FindProperty("m_bigBulletSpeed");
        m_leftPos = serializedObject.FindProperty("leftHandPos");
        m_rightPos = serializedObject.FindProperty("rightHandPos");
        m_topLeftPos = serializedObject.FindProperty("topLeftHandPos");
        m_topRightPos = serializedObject.FindProperty("topRightHandPos");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_deadKeyTrigger);
        EditorGUILayout.PropertyField(m_maxHP);
        EditorGUILayout.PropertyField(m_coin);
        EditorGUILayout.PropertyField(m_visibleRange);
        EditorGUILayout.PropertyField(m_stunTime);
        EditorGUILayout.PropertyField(m_handDamage);
        EditorGUILayout.PropertyField(m_bulletDamage);
        EditorGUILayout.PropertyField(m_bulletSpeed);
        EditorGUILayout.PropertyField(m_bigBulletDamage);
        EditorGUILayout.PropertyField(m_bigBulletSpeed);
        
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        
        EditorGUILayout.PropertyField(m_leftPos);
        EditorGUILayout.PropertyField(m_rightPos);
        EditorGUILayout.PropertyField(m_topLeftPos);
        EditorGUILayout.PropertyField(m_topRightPos);
        
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        EditorGUI.BeginChangeCheck();
        Vector2 left = Handles.PositionHandle(m_boss.leftHandPos, Quaternion.identity);
        Vector2 right = Handles.PositionHandle(m_boss.rightHandPos, Quaternion.identity);
        Vector2 topLeft = Handles.PositionHandle(m_boss.topLeftHandPos, Quaternion.identity);
        Vector2 topRight = Handles.PositionHandle(m_boss.topRightHandPos, Quaternion.identity);
        if(EditorGUI.EndChangeCheck())
        {                
            Undo.RecordObject(m_boss, "change hand positions");
            m_boss.leftHandPos = left;      
            m_boss.rightHandPos = right;
            m_boss.topLeftHandPos = topLeft;
            m_boss.topRightHandPos = topRight;    
        }
        serializedObject.ApplyModifiedProperties();
    }
}
