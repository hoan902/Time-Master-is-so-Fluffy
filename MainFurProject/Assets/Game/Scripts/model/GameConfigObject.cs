using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "game-config", menuName = "Mgif/GameConfig", order = 1)]
public class GameConfigObject : ScriptableObject
{
    public List<WorldLevelConfig> worlds;
    [SerializeField] private List<Sprite> m_playerAvatars;
    public List<Sprite> bossAvatars;    
    [SerializeField] private List<BackgroundSource> m_backgrounds;
    [SerializeField] private List<SkinConfig> m_skins;
    [SerializeField] private List<WeaponConfig> m_weapons;
    public List<BossConfig> bossLevels;

    public void UpdateBossAvatars(List<Sprite> avatars)
    {
        bossAvatars = avatars;
    }

    public void UpdatePlayerAvatars(List<Sprite> avatars)
    {
        m_playerAvatars = avatars;
    }

    public Sprite GetPlayerAvatar(string skinName)
    {
        string fullName = "avatar-" + skinName;
        foreach (Sprite sp in m_playerAvatars)
        {
            if (sp.name == fullName)
                return sp;
        }
        return null;
    }
    public WeaponConfig GetWeapon(string weaponSkinName)
    {
        foreach(WeaponConfig weapon in m_weapons)
        {
            if (weapon.weaponSkinName == weaponSkinName)
                return weapon;
        }
        return null;
    }

    public GameObject GetBackground(Season season)
    {
        foreach (BackgroundSource source in m_backgrounds)
        {
            if (season == source.season)
                return source.background;
        }

        return null;
    }

    public SkinConfig GetSkin(string skin)
    {
        foreach (SkinConfig cf in m_skins)
        {
            if (cf.skin.Equals(skin))
                return cf;
        }

        return null;
    }
    
    public WeaponConfig GetWeapon(WeaponName weaponName)
    {
        if (m_weapons.Count == 0)
            return null;
        foreach (WeaponConfig weapon in m_weapons)
        {
            if (weapon.weaponName == weaponName)
                return weapon;
        }

        return m_weapons[0];
    }

    public Weapon GetWeaponByIndex(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= m_weapons.Count)
            return m_weapons[0].weapon;
        return m_weapons[weaponIndex].weapon;
    }

    public WeaponConfig GetWeaponBySkinName(string skinName)
    {
        if (m_weapons.Count == 0)
            return null;
        foreach (WeaponConfig weapon in m_weapons)
        {
            if (weapon.weapon.skin == skinName)
                return weapon;
        }

        return m_weapons[0];
    }

    public int GetWeaponIndex(WeaponName weaponName)
    {
        for (int i = 0; i < m_weapons.Count; i++)
        {
            if (m_weapons[i].weaponName == weaponName)
                return i;
        }

        return 0;
    }
}

[Serializable]
public class WorldLevelConfig
{
    public string worldName;
    // public GameObject worldPrefab;
    // public GameObject levelPrefab;
    public Sprite background;
    public MusicByWorld musics;
    public List<LevelConfig> levels;
}

[Serializable]
public class LevelConfig
{
    public string levelName;
    public string levelPath;
}

[Serializable]
public class BackgroundSource
{
    public Season season;
    public GameObject background;
}

[Serializable]
public class MusicByWorld
{
    public AudioClip ingameMusic;
    public AudioClip homeMusic;
    public AudioClip bossFightMusic;
}

[Serializable]
public class SkinConfig
{
    public string skin;
    public AudioClip[] fightVoices;
}

[Serializable]
public class WeaponConfig
{
    public WeaponName weaponName;
    public Weapon weapon;
    public string weaponSkinName;
    public Sprite avatar;
}

[Serializable]
public class BossConfig
{
    public string bossName;
    public string levelPath;
    public AudioClip defaultMusic;
    public AudioClip bossMusic;
}
