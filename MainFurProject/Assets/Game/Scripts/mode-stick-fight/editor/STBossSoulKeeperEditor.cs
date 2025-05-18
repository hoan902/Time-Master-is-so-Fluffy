using UnityEditor;

[CustomEditor(typeof(STBossSoulKeeper))]
public class STBossSoulKeeperEditor : Editor
{
    private SerializedProperty m_skillSpawn;
    private SerializedProperty m_bodyDamage;
    private SerializedProperty m_bulletDamage;
    private SerializedProperty m_bulletSpeed;
    private SerializedProperty m_timeGrowUpBoneFly;
    private SerializedProperty m_timeDelayShootBullet;
    private SerializedProperty m_timeDelayShieldDamage;
    private SerializedProperty m_densitySoulCreep;
    private SerializedProperty m_coin;
    private SerializedProperty m_visibleRange;
    private SerializedProperty m_deadKeyTrigger;
    private SerializedProperty m_maxHP;

    private void OnEnable()
    {
        m_skillSpawn = serializedObject.FindProperty("m_skillSpawn");
        m_bodyDamage = serializedObject.FindProperty("m_bodyDamage");
        m_bulletDamage = serializedObject.FindProperty("m_bulletDamage");
        m_bulletSpeed = serializedObject.FindProperty("m_bulletSpeed");
        m_timeGrowUpBoneFly = serializedObject.FindProperty("m_timeGrowUpBoneFly");
        m_timeDelayShootBullet = serializedObject.FindProperty("m_timeDelayShootBullet");
        m_timeDelayShieldDamage = serializedObject.FindProperty("m_timeDelayShieldDamage");
        m_densitySoulCreep = serializedObject.FindProperty("m_densitySoulCreep");
        m_coin = serializedObject.FindProperty("coin");
        m_visibleRange = serializedObject.FindProperty("visibleRange");
        m_deadKeyTrigger = serializedObject.FindProperty("deadKeyTrigger");
        m_maxHP = serializedObject.FindProperty("maxHP");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_maxHP);
        EditorGUILayout.PropertyField(m_coin);
        EditorGUILayout.PropertyField(m_visibleRange);
        EditorGUILayout.PropertyField(m_skillSpawn);
        EditorGUILayout.PropertyField(m_bodyDamage);
        EditorGUILayout.PropertyField(m_bulletDamage);
        EditorGUILayout.PropertyField(m_bulletSpeed);
        EditorGUILayout.PropertyField(m_timeGrowUpBoneFly);
        EditorGUILayout.PropertyField(m_timeDelayShootBullet);
        EditorGUILayout.PropertyField(m_timeDelayShieldDamage);
        EditorGUILayout.PropertyField(m_densitySoulCreep);
        EditorGUILayout.PropertyField(m_deadKeyTrigger);

        serializedObject.ApplyModifiedProperties();
    }
}
