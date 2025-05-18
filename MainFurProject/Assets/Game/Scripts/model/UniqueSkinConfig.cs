using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "unique-skin-config", menuName = "Mgif/UniqueSkinConfig", order = 1)]
public class UniqueSkinConfig : ScriptableObject
{
    public UniqueSkin[] configs;
    public SkillDisplay[] skillDisplays;

    public bool IsUniqueSkin(string skinID)
    {
        foreach(UniqueSkin uniqueSkin in configs)
        {
            if(uniqueSkin.skinID == skinID)
                return true;
        }
        return false;
    }
    public bool SkinContainSkill(string skinID, BoosterType skill)
    {
        UniqueSkin uniqueSkin = GetUniqueSkin(skinID);
        if(uniqueSkin == null)
            return false;
        foreach(BoosterType ballSkill in uniqueSkin.skill)
        {
            if(ballSkill == skill)
                return true;
        }
        return false;
    }
    public UniqueSkin GetUniqueSkin(string skinID)
    {
        foreach(UniqueSkin config in configs)
        {
            if(config.skinID == skinID)
                return config;
        }
        return null;
    }

//     public void UpdateAllSkinID()
//     {
// #if UNITY_EDITOR
//         if(MainModel.storeConfig.unique_skins.Count != configs.Length)
//         {
//             Debug.Log("Setup thieu unique skin roi!!!");
//             Debug.Break();
//             return;
//         }
//         for(int i = 0; i < MainModel.storeConfig.unique_skins.Count; i++)
//         {
//             configs[i].skinID = MainModel.storeConfig.unique_skins[i].skin;
//         }
// #endif
//     }
}

[Serializable]
public class UniqueSkin
{
    public string skinID;
    public BoosterType[] skill;
    public int condition = 5;
    public bool isSpecial;
}
[Serializable]
public class SkillDisplay
{
    public BoosterType boosterType;
    public GameObject topRightIcon;
    public GameObject underBallIcon;
    public GameObject overBallIcon;
    public GameObject effect;
}
