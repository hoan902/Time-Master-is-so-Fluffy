using UnityEditor;

[CustomEditor(typeof(STBossBoneLord))]
public class STBossBoneLordEditor : Editor
{
    private STBossBoneLord m_boss;

    private SerializedProperty m_deadKeyTrigger;
    private SerializedProperty m_maxHP;
    private SerializedProperty m_coin;
    private SerializedProperty m_visibleRange;
    private SerializedProperty m_nextPhaseRatio;
    private SerializedProperty m_timeScaleAnimNextPhase;
    private SerializedProperty m_moveSpeed;
    private SerializedProperty m_damage;
    private SerializedProperty m_bulletDamage;
    private SerializedProperty m_rainStoneDamage;
    private SerializedProperty m_followTargetUndergroundTime;
    private SerializedProperty m_rainStoneTime;
    private SerializedProperty m_waitFinishSkill3Time;
    private SerializedProperty m_densityBullet;
    private SerializedProperty m_densityRainStone;
    private SerializedProperty m_densityStoneRiseUp;
    private SerializedProperty m_rainStoneGravity;
    private SerializedProperty m_maxMonsterSpawn;
    private SerializedProperty m_maxBulletHeight;

    private void OnEnable()
    {
        m_boss = target as STBossBoneLord;

        m_deadKeyTrigger = serializedObject.FindProperty("deadKeyTrigger");
        m_maxHP = serializedObject.FindProperty("maxHP");
        m_coin = serializedObject.FindProperty("coin");
        m_visibleRange = serializedObject.FindProperty("visibleRange");
        m_nextPhaseRatio = serializedObject.FindProperty("m_nextPhaseRatio");
        m_timeScaleAnimNextPhase = serializedObject.FindProperty("m_timeScaleAnimNextPhase");
        m_moveSpeed = serializedObject.FindProperty("m_moveSpeed");

        m_damage = serializedObject.FindProperty("m_damage");
        m_bulletDamage = serializedObject.FindProperty("m_bulletDamage");
        m_rainStoneDamage = serializedObject.FindProperty("m_rainStoneDamage");

        m_followTargetUndergroundTime = serializedObject.FindProperty("m_followTargetUndergroundTime");
        m_rainStoneTime = serializedObject.FindProperty("m_rainStoneTime");
        m_waitFinishSkill3Time = serializedObject.FindProperty("m_waitFinishSkill3Time");

        m_densityBullet = serializedObject.FindProperty("m_densityBullet");
        m_densityRainStone = serializedObject.FindProperty("m_densityRainStone");
        m_densityStoneRiseUp = serializedObject.FindProperty("m_densityStoneRiseUp");
        m_rainStoneGravity = serializedObject.FindProperty("m_rainStoneGravity");
        m_maxMonsterSpawn = serializedObject.FindProperty("m_maxMonsterSpawn");
        m_maxBulletHeight = serializedObject.FindProperty("m_maxBulletHeight");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_deadKeyTrigger);
        EditorGUILayout.PropertyField(m_maxHP);
        EditorGUILayout.PropertyField(m_coin);
        EditorGUILayout.PropertyField(m_visibleRange);
        EditorGUILayout.PropertyField(m_nextPhaseRatio);
        EditorGUILayout.PropertyField(m_timeScaleAnimNextPhase);
        EditorGUILayout.PropertyField(m_moveSpeed);

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(m_damage);
        EditorGUILayout.PropertyField(m_bulletDamage);
        EditorGUILayout.PropertyField(m_rainStoneDamage);
        EditorGUILayout.PropertyField(m_followTargetUndergroundTime);
        EditorGUILayout.PropertyField(m_rainStoneTime);
        EditorGUILayout.PropertyField(m_waitFinishSkill3Time);

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(m_densityBullet);
        EditorGUILayout.PropertyField(m_densityRainStone);
        EditorGUILayout.PropertyField(m_densityStoneRiseUp);

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(m_rainStoneGravity);
        EditorGUILayout.PropertyField(m_maxMonsterSpawn);
        EditorGUILayout.PropertyField(m_maxBulletHeight);

        serializedObject.ApplyModifiedProperties();
    }
}
