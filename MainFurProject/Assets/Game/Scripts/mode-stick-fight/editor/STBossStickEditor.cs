using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(STBossStick))]
public class STBossStickEditor : Editor
{   
    private STBossStick m_boss;
    
    private SerializedProperty m_maxHP;
    private SerializedProperty m_visibleRange;
    private SerializedProperty m_moveSpeed;
    private SerializedProperty m_monsterSpawnPositions;
    private SerializedProperty m_bulletSpeed;
    private SerializedProperty m_coin;
    private SerializedProperty m_deadKeyTrigger;
    private SerializedProperty m_leftPos;
    private SerializedProperty m_rightPos;
    private SerializedProperty m_topPos;
    private SerializedProperty m_bottomPos;
    private SerializedProperty m_upperPos;
    private SerializedProperty m_lowerPos;
    private SerializedProperty m_stunTime;
    private SerializedProperty m_smallBulletDamage;
    private SerializedProperty m_bigBulletDamage;
    private SerializedProperty m_waveDamage;
    private SerializedProperty m_bodyDamage;
    private SerializedProperty m_monsterScoutAmount;
    private SerializedProperty m_monsterPlinkyAmount;

    private void OnEnable() 
    {
        m_boss = target as STBossStick;

        m_maxHP = serializedObject.FindProperty("maxHP");
        m_visibleRange = serializedObject.FindProperty("visibleRange");
        m_moveSpeed = serializedObject.FindProperty("m_moveSpeed");
        m_monsterSpawnPositions = serializedObject.FindProperty("monsterSpawnPostitions");
        m_bulletSpeed = serializedObject.FindProperty("m_bulletSpeed");
        m_coin = serializedObject.FindProperty("coin");
        m_deadKeyTrigger = serializedObject.FindProperty("deadKeyTrigger");
        m_leftPos = serializedObject.FindProperty("leftPos");
        m_rightPos = serializedObject.FindProperty("rightPos");
        m_topPos = serializedObject.FindProperty("topPos");
        m_bottomPos = serializedObject.FindProperty("bottomPos");
        m_upperPos = serializedObject.FindProperty("upperPos");
        m_lowerPos = serializedObject.FindProperty("lowerPos");
        m_stunTime = serializedObject.FindProperty("m_stunTime");
        m_bodyDamage = serializedObject.FindProperty("m_damage");
        m_smallBulletDamage = serializedObject.FindProperty("m_smallBulletDamamge");
        m_bigBulletDamage = serializedObject.FindProperty("m_bigBulletDamage");
        m_waveDamage = serializedObject.FindProperty("m_waveBulletDamage");
        m_monsterScoutAmount = serializedObject.FindProperty("m_monsterScoutAmount");
        m_monsterPlinkyAmount = serializedObject.FindProperty("m_monsterPlinkyAmount");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_deadKeyTrigger);
        EditorGUILayout.PropertyField(m_maxHP);
        EditorGUILayout.PropertyField(m_stunTime);
        EditorGUILayout.PropertyField(m_moveSpeed);
        EditorGUILayout.PropertyField(m_bulletSpeed);
        EditorGUILayout.PropertyField(m_coin);
        EditorGUILayout.PropertyField(m_visibleRange);
        EditorGUILayout.PropertyField(m_bodyDamage);
        EditorGUILayout.PropertyField(m_monsterScoutAmount);
        EditorGUILayout.PropertyField(m_monsterPlinkyAmount);
        EditorGUILayout.PropertyField(m_smallBulletDamage);
        EditorGUILayout.PropertyField(m_bigBulletDamage);
        EditorGUILayout.PropertyField(m_waveDamage);
        
        EditorGUILayout.PropertyField(m_monsterSpawnPositions);
        EditorGUILayout.PropertyField(m_leftPos);
        EditorGUILayout.PropertyField(m_rightPos);
        EditorGUILayout.PropertyField(m_topPos);
        EditorGUILayout.PropertyField(m_bottomPos);
        EditorGUILayout.PropertyField(m_upperPos);
        EditorGUILayout.PropertyField(m_lowerPos);

        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        if(m_boss.monsterSpawnPostitions == null)
            return;
        Vector2[] path = m_boss.monsterSpawnPostitions;
        for(int i = 0; i < path.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector2 pos = Handles.PositionHandle(path[i], Quaternion.identity);
            if(EditorGUI.EndChangeCheck())
            {                
                Undo.RecordObject(m_boss, "change path");
                m_boss.monsterSpawnPostitions[i] = pos;                
            }
        }
        serializedObject.ApplyModifiedProperties();

        EditorGUI.BeginChangeCheck();
        Vector2 left = Handles.PositionHandle(m_boss.leftPos, Quaternion.identity);
        Vector2 right = Handles.PositionHandle(m_boss.rightPos, Quaternion.identity);
        Vector2 top = Handles.PositionHandle(m_boss.topPos, Quaternion.identity);
        Vector2 bottom = Handles.PositionHandle(m_boss.bottomPos, Quaternion.identity);
        Vector2 upper = Handles.PositionHandle(m_boss.upperPos, Quaternion.identity);
        Vector2 lower = Handles.PositionHandle(m_boss.lowerPos, Quaternion.identity);
        if(EditorGUI.EndChangeCheck())
        {                
            Undo.RecordObject(m_boss, "change border position");
            m_boss.leftPos = left;      
            m_boss.rightPos = right;
            m_boss.topPos = top;
            m_boss.bottomPos = bottom;    
            m_boss.upperPos = upper;
            m_boss.lowerPos = lower;    
        }
        serializedObject.ApplyModifiedProperties();
    }
}
