using UnityEditor;

[CustomEditor(typeof(STBossGoldhorn))]
public class STBossGoldhornEditor : Editor
{
    private SerializedProperty m_runSpeed;
    private SerializedProperty m_goresSpeed;
    private SerializedProperty m_bodyDamage;
    private SerializedProperty m_trailDamage;
    private SerializedProperty m_timeJump;
    private SerializedProperty m_rangeJump;
    private SerializedProperty m_jumpPower;
    private SerializedProperty m_coin;
    private SerializedProperty m_visibleRange;
    private SerializedProperty m_deadKeyTrigger;
    private SerializedProperty m_maxHP;
    private SerializedProperty m_knockbackDirectionOffset;
    private SerializedProperty m_nextPhaseRatio;

    private void OnEnable()
    {
        m_runSpeed = serializedObject.FindProperty("m_runSpeed");
        m_goresSpeed = serializedObject.FindProperty("m_goresSpeed");
        m_bodyDamage = serializedObject.FindProperty("m_bodyDamage");
        m_trailDamage = serializedObject.FindProperty("m_trailDamage");
        m_timeJump = serializedObject.FindProperty("m_timeJump");
        m_rangeJump = serializedObject.FindProperty("m_rangeJump");
        m_jumpPower = serializedObject.FindProperty("m_jumpPower");
        m_nextPhaseRatio = serializedObject.FindProperty("m_nextPhaseRatio");
        m_coin = serializedObject.FindProperty("coin");
        m_visibleRange = serializedObject.FindProperty("visibleRange");
        m_deadKeyTrigger = serializedObject.FindProperty("deadKeyTrigger");
        m_maxHP = serializedObject.FindProperty("maxHP");
        m_knockbackDirectionOffset = serializedObject.FindProperty("knockbackDirectionOffset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_maxHP);
        EditorGUILayout.PropertyField(m_coin);
        EditorGUILayout.PropertyField(m_knockbackDirectionOffset);
        EditorGUILayout.PropertyField(m_visibleRange);
        EditorGUILayout.PropertyField(m_nextPhaseRatio);
        EditorGUILayout.PropertyField(m_bodyDamage);
        EditorGUILayout.PropertyField(m_trailDamage);
        EditorGUILayout.PropertyField(m_runSpeed);
        EditorGUILayout.PropertyField(m_goresSpeed);
        EditorGUILayout.PropertyField(m_timeJump);
        EditorGUILayout.PropertyField(m_rangeJump);
        EditorGUILayout.PropertyField(m_jumpPower);
        EditorGUILayout.PropertyField(m_deadKeyTrigger);

        serializedObject.ApplyModifiedProperties();
    }
}
