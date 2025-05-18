using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerPrefController : MonoBehaviour
{
    // Booster Store
    public static void SetBoosterAmount(BoosterType boosterType, int value)
    {
        PlayerPrefs.SetInt(boosterType.ToString(), value);
    }
    public static int GetBoosterAmount(BoosterType boosterType)
    {
        return PlayerPrefs.GetInt(boosterType.ToString(), 0);
    }
    public static void SetAdsWatched(int index, int value)
    {
        PlayerPrefs.SetInt("item" + index, value);
    }
    public static int GetAdsWatched(int index)
    {
        return PlayerPrefs.GetInt("item" + index, 0);
    }
    public static void SetAdsWatchedHeartItem(int index, int value)
    {
        PlayerPrefs.SetInt("item-heart-" + index, value);
    }
    public static int GetAdsWatchedHeartItem(int index)
    {
        return PlayerPrefs.GetInt("item-heart-" + index, 0);
    }
    public static void SetSoldOutList(List<int> soldOutList)
    {
        PlayerPrefs.SetString(DataKey.SOLD_OUT_BOOSTER, string.Join(",", soldOutList));
    }
    public static List<int> GetSoldOutList()
    {
        List<int> result = new List<int>();
        List<string> s = PlayerPrefs.GetString(DataKey.SOLD_OUT_BOOSTER, "-1").Split(',').ToList();      
        foreach(string element in s)
        {
            result.Add(System.Int32.Parse(element));
        }
        return result;
    }
    public static void SetSoldOutHeartList(List<int> soldOutList)
    {
        PlayerPrefs.SetString(DataKey.SOLD_OUT_HEART, string.Join(",", soldOutList));
    }
    public static List<int> GetSoldOutHeartList()
    {
        List<int> result = new List<int>();
        List<string> s = PlayerPrefs.GetString(DataKey.SOLD_OUT_HEART, "-1").Split(',').ToList();      
        foreach(string element in s)
        {
            result.Add(System.Int32.Parse(element));
        }
        return result;
    }
    // Unique Skin Store
    public static void SetAdsWatchedUniqueSkin(string skinID, int value)
    {
        PlayerPrefs.SetInt(DataKey.PREFIX_BALL_SKILL + skinID, value);
        PlayerPrefs.Save();
    }
    public static int GetAdsWatchedUniqueSkin(string skinID)
    {
        return PlayerPrefs.GetInt(DataKey.PREFIX_BALL_SKILL + skinID, 0);
    }
    public static void SetGotUniqueSkin(List<string> gotUniqueSkinList)
    {
        PlayerPrefs.SetString(DataKey.GOT_UNIQUE_SKINS, string.Join(",", gotUniqueSkinList));
        PlayerPrefs.Save();
    }
    public static List<string> GetGotUniqueSkin()
    {
        List<string> result = new List<string>();
        List<string> s = PlayerPrefs.GetString(DataKey.GOT_UNIQUE_SKINS, "-1").Split(',').ToList();      
        foreach(string element in s)
        {
            result.Add(element);
        }
        return result;
    }

    // Keep Combo
    public static void SetAdsWatchedKeepCombo(string heroName, int value)
    {
        PlayerPrefs.SetInt(DataKey.COMBO_ADS_WATCHED_PREFIX + heroName, value);
        PlayerPrefs.Save();
    }
    public static int GetAdsWatchedKeepCombo(string heroName)
    {
        return PlayerPrefs.GetInt(DataKey.COMBO_ADS_WATCHED_PREFIX + heroName, 0);
    }

    // Leader board
    public static void SetHistoryLevel (int level, (int highScore, int star) value)
    {
        PlayerPrefs.SetInt("history-highscore-" + level, value.highScore);
        PlayerPrefs.SetInt("history-star-" + level, value.star);
        PlayerPrefs.Save();
    }

    public static (int hightScore, int star) GetHistoryLevel (int level)
    {
        (int, int) value = (0, 0);
        value.Item1 = PlayerPrefs.GetInt("history-highscore-" + level, 0);
        value.Item2 = PlayerPrefs.GetInt("history-star-" + level, 0);
        return value;
    }

    // Cutscene
    public static void SaveAllPlayedCutscene(string cutsceneToSave)
    {
        List<string> result = GetAllPlayedCutscene();
        if(result.Contains(cutsceneToSave))
            return;
        result.Add(cutsceneToSave);
        string text = string.Join(",", result);
        PlayerPrefs.SetString(DataKey.PLAYED_CUTSCENE, text);
        PlayerPrefs.Save();
    }
    public static List<string> GetAllPlayedCutscene()
    {
        List<string> result = new List<string>();
        string text = PlayerPrefs.GetString(DataKey.PLAYED_CUTSCENE, "");
        if(text != "")
        {
            string[] arr = text.Split(",");
            for(int i = 0; i < arr.Length; i++)
            {
                result.Add(arr[i]);
            }
        }
        return result;
    }
}
