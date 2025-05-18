using UnityEditor;

[CustomEditor(typeof(STBossJungleGuardian))]
public class STBossJungleGuardianEditor : Editor
{
    private SerializedProperty m_bodyDamage;
    private SerializedProperty m_bulletDamage;
    private SerializedProperty m_thunderDamage;
    private SerializedProperty m_speedBullet;
    private SerializedProperty m_maxHeightCannonShoot;
    private SerializedProperty m_timeDelayIdleSkill;
    private SerializedProperty m_timeDelayBulletShoot;
    private SerializedProperty m_timeDelayBulletCannon;
    private SerializedProperty m_rainThunderTime;
    private SerializedProperty m_densityBulletShoot;
    private SerializedProperty m_densityBulletCannon;
    private SerializedProperty m_densityThunder;
    private SerializedProperty m_coin;
    private SerializedProperty m_visibleRange;
    private SerializedProperty m_deadKeyTrigger;
    private SerializedProperty m_maxHP;
    private SerializedProperty m_nextPhaseRatio;

    private void OnEnable()
    {
        m_bodyDamage = serializedObject.FindProperty("m_bodyDamage");
        m_bulletDamage = serializedObject.FindProperty("m_bulletDamage");
        m_thunderDamage = serializedObject.FindProperty("m_thunderDamage");
        m_speedBullet = serializedObject.FindProperty("m_speedBullet");
        m_maxHeightCannonShoot = serializedObject.FindProperty("m_maxHeightCannonShoot");

        m_timeDelayIdleSkill = serializedObject.FindProperty("m_timeDelayIdleSkill");
        m_timeDelayBulletShoot = serializedObject.FindProperty("m_timeDelayBulletShoot");
        m_timeDelayBulletCannon = serializedObject.FindProperty("m_timeDelayBulletCannon");
        m_rainThunderTime = serializedObject.FindProperty("m_rainThunderTime");
        m_densityBulletShoot = serializedObject.FindProperty("m_densityBulletShoot");
        m_densityBulletCannon = serializedObject.FindProperty("m_densityBulletCannon");
        m_densityThunder = serializedObject.FindProperty("m_densityThunder");

        m_nextPhaseRatio = serializedObject.FindProperty("m_nextPhaseRatio");
        m_coin = serializedObject.FindProperty("coin");
        m_visibleRange = serializedObject.FindProperty("visibleRange");
        m_deadKeyTrigger = serializedObject.FindProperty("deadKeyTrigger");
        m_maxHP = serializedObject.FindProperty("maxHP");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_deadKeyTrigger);
        EditorGUILayout.PropertyField(m_maxHP);
        EditorGUILayout.PropertyField(m_coin);
        EditorGUILayout.PropertyField(m_visibleRange);
        EditorGUILayout.PropertyField(m_nextPhaseRatio);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_bodyDamage);
        EditorGUILayout.PropertyField(m_bulletDamage);
        EditorGUILayout.PropertyField(m_thunderDamage);
        EditorGUILayout.PropertyField(m_speedBullet);
        EditorGUILayout.PropertyField(m_maxHeightCannonShoot);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_timeDelayIdleSkill);
        EditorGUILayout.PropertyField(m_timeDelayBulletShoot);
        EditorGUILayout.PropertyField(m_timeDelayBulletCannon);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_rainThunderTime);
        EditorGUILayout.PropertyField(m_densityBulletShoot);
        EditorGUILayout.PropertyField(m_densityBulletCannon);
        EditorGUILayout.PropertyField(m_densityThunder);

        serializedObject.ApplyModifiedProperties();
    }
}
