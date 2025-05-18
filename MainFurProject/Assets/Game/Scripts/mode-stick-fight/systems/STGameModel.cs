using UnityEngine;

public class STGameModel : MonoBehaviour
{
    private static int m_hp;
    public static bool immunity;
    public static WeaponName currentWeapon;
    
    public static int hp => m_hp;

    public static void Init()
    {
        m_hp = STGameConstant.PLAYER_MAX_HEALTH;
        immunity = false;
    }

    public static void UpdateHp(int value)
    {
        m_hp += value;
        m_hp = Mathf.Clamp(m_hp, -1, STGameConstant.PLAYER_MAX_HEALTH);
    }
}
