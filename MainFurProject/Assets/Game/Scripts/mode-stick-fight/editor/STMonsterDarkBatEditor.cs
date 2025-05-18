using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

[CustomEditor(typeof(STMonsterDarkBat))]
public class STMonsterDarkBatEditor : Editor
{
    private STMonsterDarkBat m_monster;
    
    private SerializedProperty m_maxHP;
    private SerializedProperty m_knockbackStrenght;
    private SerializedProperty m_flySpeed;
    private SerializedProperty m_readyAttackSpeed;
    private SerializedProperty m_attackDuration;
    private SerializedProperty m_bodyDamage;
    private SerializedProperty m_needleDamage;
    private SerializedProperty m_followRangeLower;
    private SerializedProperty m_followRangeHigher;
    private SerializedProperty m_readyAttackRangeLower;
    private SerializedProperty m_readyAttackRangeHigher;
    
    private float m_visionSize;
    private float m_deactiveSize;
    private Vector2 m_visionOffset;
    
    private void OnEnable()
    {
        m_monster = target as STMonsterDarkBat;
        
        m_maxHP = serializedObject.FindProperty("maxHP");
        m_knockbackStrenght = serializedObject.FindProperty("knockbackStrength");
        m_flySpeed = serializedObject.FindProperty("m_flySpeed");
        m_readyAttackSpeed = serializedObject.FindProperty("m_readyAttackSpeed");
        m_attackDuration = serializedObject.FindProperty("m_attackDuration");
        m_bodyDamage = serializedObject.FindProperty("m_bodyDamage");
        m_needleDamage = serializedObject.FindProperty("m_needleDamage");
        m_followRangeLower = serializedObject.FindProperty("m_followRangeLower");
        m_followRangeHigher = serializedObject.FindProperty("m_followRangeHigher");
        m_readyAttackRangeLower = serializedObject.FindProperty("m_readyAttackRangeLower");
        m_readyAttackRangeHigher = serializedObject.FindProperty("m_readyAttackRangeHigher");

        m_visionSize = m_monster.activeRangeCollider.radius;
        m_visionOffset = m_monster.activeRangeCollider.offset;
        m_deactiveSize = m_monster.deactiveRangeCollider.radius;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_maxHP);
        EditorGUILayout.PropertyField(m_knockbackStrenght);
        EditorGUILayout.PropertyField(m_flySpeed);
        EditorGUILayout.PropertyField(m_readyAttackSpeed);
        EditorGUILayout.PropertyField(m_attackDuration);
        EditorGUILayout.PropertyField(m_bodyDamage);
        EditorGUILayout.PropertyField(m_needleDamage);
        
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(m_followRangeLower);
        EditorGUILayout.PropertyField(m_followRangeHigher);
        EditorGUILayout.PropertyField(m_readyAttackRangeLower);
        EditorGUILayout.PropertyField(m_readyAttackRangeHigher);
        
        EditorGUI.BeginChangeCheck();
        m_visionSize = EditorGUILayout.FloatField("Active Range", m_visionSize);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            m_monster.UpdateActiveAreaSize(m_visionSize);
            EditorUtility.SetDirty(target);
        }
        
        EditorGUI.BeginChangeCheck();
        m_visionOffset = EditorGUILayout.Vector2Field("Active Area Offset", m_visionOffset);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            m_monster.UpdateActiveAreaOffset(m_visionOffset);
            EditorUtility.SetDirty(target);
        }

        EditorGUI.BeginChangeCheck();
        m_deactiveSize = EditorGUILayout.FloatField("Deactive Range", m_deactiveSize);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            m_monster.UpdateDeactiveSize(m_deactiveSize);
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
