using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using System.Linq;

public class ConfigLoader : MonoBehaviour
{
    public static ConfigLoader instance;
    
    public AssetReference configAsset;
    public AssetReference uniqueSkinConfigAsset;
    public AssetReference buttonStyleConfigAsset;
    public AssetReference skinConfigAsset;

    [HideInInspector] public GameConfigObject config;
    [HideInInspector] public CutsceneButtonStyleConfig buttonStyleConfig;
    
    public List<WorldLevelConfig> worlds
    {
        get
        {
            return config.worlds;
        }
    }

    public int mapLevel
    {
        get
        {
            return PlayerPrefs.GetInt(DataKey.MAP_LEVEL, 0);
        }
        set
        {
            PlayerPrefs.SetInt(DataKey.MAP_LEVEL, value);
            PlayerPrefs.Save();
        }
    }

    public int worldLevel
    {
        get
        {
            return PlayerPrefs.GetInt(DataKey.WORLD_LEVEL, 0);
        }
        set
        {
            PlayerPrefs.SetInt(DataKey.WORLD_LEVEL, value);
            PlayerPrefs.Save();
        }
    }

    public int fakeMapLevel
    {
        get
        {
            return GetFakeLevelIndex(mapLevel, worldLevel);
        }
    }

    public WorldLevelConfig currentWorld
    {
        get
        {
            int index = worldLevel % worlds.Count;
            return worlds[index];
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if(instance == null)
            instance = this;
    }

    public List<int> LoadPoints(int world)
    {
        List<int> res = new List<int>();
        string text = PlayerPrefs.GetString(DataKey.WORLD_POINT + world, "");
        if (text != "")
        {
            string[] arr = text.Split(",");
            for (int i = 0; i < arr.Length; i++)
            {
                res.Add(int.Parse(arr[i]));
            }
        }
        return res;
    }

    public void SavePoint(int world, int level, int point)
    {
        List<int> res = LoadPoints(world);
        if (res.Count > level)
            res[level] = Mathf.Max(res[level], point);
        else
            res.Add(point);
        string text = string.Join(",", res);
        PlayerPrefs.SetString(DataKey.WORLD_POINT + world, text);
        PlayerPrefs.Save();
    }

    public List<int> LoadRamadanPoints()
    {
        List<int> res = new List<int>();
        string text = PlayerPrefs.GetString(DataKey.RAMADAN_POINT, "");
        if (text != "")
        {
            string[] arr = text.Split(",");
            for (int i = 0; i < arr.Length; i++)
            {
                res.Add(int.Parse(arr[i]));
            }
        }
        return res;
    }
    public void SaveRamadanPoint(int level, int point)
    {
        List<int> res = LoadRamadanPoints();
        if (res.Count > level)
            res[level] = Mathf.Max(res[level], point);
        else
            res.Add(point);
        string text = string.Join(",", res);
        PlayerPrefs.SetString(DataKey.RAMADAN_POINT, text);
        PlayerPrefs.Save();
    }

    public List<int> LoadRedBluePoints()
    {
        List<int> res = new List<int>();
        string text = PlayerPrefs.GetString(DataKey.RED_BLUE_POINT, "");
        if (text != "")
        {
            string[] arr = text.Split(",");
            for (int i = 0; i < arr.Length; i++)
            {
                res.Add(int.Parse(arr[i]));
            }
        }
        return res;
    }
    public void SaveRedBluePoint(int level, int point)
    {
        List<int> res = LoadRedBluePoints();
        if (res.Count > level)
            res[level] = Mathf.Max(res[level], point);
        else
            res.Add(point);
        string text = string.Join(",", res);
        PlayerPrefs.SetString(DataKey.RED_BLUE_POINT, text);
        PlayerPrefs.Save();
    }

    public List<int> LoadStickPoint()
    {
        List<int> res = new List<int>();
        string text = PlayerPrefs.GetString(DataKey.STICK_POINT, "");
        if (text != "")
        {
            string[] arr = text.Split(",");
            for (int i = 0; i < arr.Length; i++)
            {
                res.Add(int.Parse(arr[i]));
            }
        }
        return res;
    }
    public void SaveStickPoint(int level, int point)
    {
        List<int> res = LoadStickPoint();
        if (res.Count > level)
            res[level] = Mathf.Max(res[level], point);
        else
            res.Add(point);
        string text = string.Join(",", res);
        PlayerPrefs.SetString(DataKey.STICK_POINT, text);
        PlayerPrefs.Save();
    }

    public List<PlayMode> LoadPlayedModes()
    {
        List<PlayMode> result = new List<PlayMode>();
        string text = PlayerPrefs.GetString(DataKey.MODE_PLAYED, "");
        if(text != "")
        {
            string[] arr = text.Split(",");
            for (int i = 0; i < arr.Length; i++)
            {
                result.Add(Enum.Parse<PlayMode>(arr[i]));
            }
        }
        return result;
    }
    public void SavePlayedModes(PlayMode mode, bool toAdd)
    {
        List<PlayMode> list = LoadPlayedModes();
        if(list.Contains(mode) == toAdd)
            return;
        if(toAdd)
            list.Add(mode);
        else
            list.Remove(mode);
        string text = string.Join(",", list);
        PlayerPrefs.SetString(DataKey.MODE_PLAYED, text);
        PlayerPrefs.Save();
    }
    public List<PlayMode> LoadIntroducedModes()
    {
        List<PlayMode> result = new List<PlayMode>();
        string text = PlayerPrefs.GetString(DataKey.MODE_INTRODUCED, "");
        if(text != "")
        {
            string[] arr = text.Split(",");
            for (int i = 0; i < arr.Length; i++)
            {
                result.Add(Enum.Parse<PlayMode>(arr[i]));
            }
        }
        return result;
    }
    public void SaveIntroducedModes(PlayMode mode)
    {
        List<PlayMode> list = LoadIntroducedModes();
        if(list.Contains(mode))
            return;
        list.Add(mode);
        string text = string.Join(",", list);
        PlayerPrefs.SetString(DataKey.MODE_INTRODUCED, text);
        PlayerPrefs.Save();
    }

    public WorldLevelConfig GetWorldByIndex(int index)
    {
        int realIndex = index % worlds.Count;
        if(realIndex < 0)
            return null;
        return worlds[realIndex];
    }

    public WorldLevelConfig GetWorldByLevelName(string levelName)
    {
        foreach(WorldLevelConfig worldLevelConfig in worlds)
        {
            foreach(LevelConfig levelConfig in worldLevelConfig.levels)
            {
                if(("level-" + levelConfig.levelPath) == levelName)
                    return worldLevelConfig;
            }
        }
        return worlds[0];
    }

    public int GetWorldIndex(WorldLevelConfig worldLevelConfig)
    {
        for(int i = 0; i < worlds.Count; i++)
        {
            if(worldLevelConfig == worlds[i])
                return i;
        }
        return 0;
    }
    public int GetLevelIndexByName(string levelName, int worldIndex)
    {
        for(int i = 0; i < worlds[worldIndex].levels.Count; i++)
        {
            if(levelName.Replace("level-", "") ==  worlds[worldIndex].levels[i].levelPath)
                return i;
        }
        return 0;
    }

    public LevelConfig GetLevelConfig(int level)
    {
        WorldLevelConfig world = currentWorld;
        return level >= world.levels.Count ? world.levels[world.levels.Count - 1] : world.levels[level];
    }

    public LevelConfig GetLevel(int worldIndex, int levelIndex, PlayMode playMode = PlayMode.Normal)
    {
        switch (playMode)
        {
            case PlayMode.Normal:
                int index = worldIndex % worlds.Count;
                WorldLevelConfig world = worlds[index];
                if (world == null)
                    return null;
                return levelIndex >= world.levels.Count ? world.levels[world.levels.Count - 1] : world.levels[levelIndex];
            case PlayMode.Boss:
                BossConfig boss = config.bossLevels[levelIndex];
                return new LevelConfig()
                {
                    levelName = boss.bossName,
                    levelPath = boss.levelPath
                };
                break;
            default:
                return null;
        }

        return null;
    }
    
    public List<BossInfo> GetAllBoss()
    {
        Dictionary<string, BossInfo> result = new Dictionary<string, BossInfo>();
        int levelCounter = 0;
        int maxWorld = worlds.Count;
        for (int i = 0; i < maxWorld; i++)
        {
            int maxLevel = worlds[i].levels.Count;
            for (int j = 0; j < maxLevel; j++)
            {
                string levelPath = worlds[i].levels[j].levelPath;
                if (IsBossLevel(levelPath))
                {
                    string bossName = GetBossName(levelPath);
                    if(result.ContainsKey(bossName))
                        continue;
                    BossInfo info = new BossInfo()
                    {
                        bossName = bossName,
                        unlockLevel = levelCounter,
                        avatar = FindBossAvatar(bossName)
                    };
                    result.Add(bossName, info);
                }

                levelCounter++;
            }
        }
        return result.Values.ToList();
    }

    public MapInfo GetNextBossLevel(int worldIndex, int levelIndex)
    {
        int currentLevel = levelIndex;
        int realWorldIndex = worldIndex % worlds.Count;
        int counter = 1000;
        while (counter > 0)
        {
            for (int i = realWorldIndex; i < worlds.Count; i++)
            {
                WorldLevelConfig world = worlds[i];
                for (int j = currentLevel; j < world.levels.Count; j++)
                {
                    if (IsBossLevel(world.levels[j].levelPath))
                        return new MapInfo() { world = i, level = j};                    
                }
                currentLevel = 0;
            }
            realWorldIndex = 0;
            counter--;
        }
        return null;
    }

    public MapInfo GetBeginLevelProgress(int worldIndex, int levelIndex)
    {        
        int startWorld = worldIndex % worlds.Count;
        int startLevel = levelIndex - 1; 
        int counter = 1000;
        int levelCounter = GetFakeLevelIndex(levelIndex, worldIndex) - 1;
        if (levelCounter < 1)
            return new MapInfo() { world = 0, level = 0 };
        while (counter > 0)
        {
            for (int i = startWorld; i > -1; i--)
            {
                WorldLevelConfig world = worlds[i];
                for (int j = startLevel; j > -1; j--)
                {
                    if (IsBossLevel(world.levels[j].levelPath))
                        return new MapInfo() { world = i, level = j + 1 };
                    if (levelCounter < 1)
                        return new MapInfo() { world = 0, level = 0 };
                    levelCounter--;
                }
                startLevel = worlds[i == 0 ? (worlds.Count - 1): (i-1)].levels.Count - 1;
            }
            startWorld = config.worlds.Count - 1;
            counter--;
        }
        return null;
    }

    public bool IsBossLevel(int level, int world)
    {
        LevelConfig levelConfig = GetLevel(level, world);
        if (levelConfig == null)
            return false;
        return IsBossLevel(levelConfig.levelPath);
    }

    public Sprite FindBossAvatar(string bossName)
    {
        foreach(Sprite s in config.bossAvatars)
        {
            if (s.name == bossName)
                return s;
        }
        return null;
    }

    public int GetFakeLevelIndex(int level, int world)
    {
        int counter = 0;
        int levelCounter = 0;
        int stop = 1000;
        while (counter <= world && stop > 0)
        {
            for(int i = 0; i < worlds.Count; i++)
            {
                for(int j = 0; j < worlds[i].levels.Count; j++)
                {                    
                    if (counter == world && j == level)
                        return levelCounter;
                    levelCounter++;
                }
                counter++;
            }
            stop--;
        }
        return 0;
    }

    public void SkipLevel(int level, int world)
    {
        int currentWorldIndex = worldLevel;
        int currentLevel = mapLevel;
        if (world < currentWorldIndex || (world == currentWorldIndex && level < currentLevel))
            return;
        WorldLevelConfig current = currentWorld;        
        int levelIndex = currentLevel + 1;
        if (level >= (current.levels.Count - 1))
        {
            levelIndex = 0;
            currentWorldIndex++;
        }
        mapLevel = levelIndex;
        worldLevel = currentWorldIndex;
    }

    public void SaveMapLevel(int level, int world, int point, PlayMode playMode)
    {
        switch(playMode)
        {
            case PlayMode.Normal:
                SavePoint(world, level, point);
                break;
        }
        if (level != mapLevel || worldLevel != world || playMode != PlayMode.Normal)
            return;        
        SkipLevel(level, world);
    }

    public void CheatLevel(int level, int world)
    {        
        worldLevel = world;
        int worldIndex = world%worlds.Count;
        mapLevel = Mathf.Min(level, worlds[worldIndex].levels.Count - 1);
        for(int i = 0; i <= world; i++)
        {
            List<int> res = LoadPoints(i);
            int max = GetWorldByIndex(i).levels.Count;
            for (int j = res.Count; j < max; j++)
            {
                SavePoint(i, j, 0);
            }
        }
    }

    public bool IsReplay(int world, int level)
    {
        return world != worldLevel || level != mapLevel;
    }

    /// ////////////////////////////////////////////////////////////////
    public static bool IsBossLevel(string levelName)
    {
        return levelName.ToLower().Contains("boss");
    }

    public static bool IsBonusLevel(string levelName)
    {
        return levelName.ToLower().Contains("bonus");
    }

    public static bool IsSkinLevel(string levelName)
    {
        return levelName.ToLower().Contains("skin");
    }

    public static string GetBossName(string levelPath)
    {
        int startIndex = levelPath.ToLower().IndexOf("boss");
        if (startIndex < 0)
            return "";
        string longName = levelPath.Substring(startIndex);
        int last = longName.IndexOf("_");
        return longName.Substring(0, last < 0 ? longName.Length : last);
    }

    public string GetCurrentLevel()
    {
        string levelName = GetLevel(worldLevel, mapLevel).levelPath;
        return levelName;
    }

    public static string GetLevelString(int world, int level, string prefix = "Level ")
    { 
        int fakeLevel = instance.GetFakeLevelIndex(level, world);
        return fakeLevel == 0 ? "Tutorial" : (prefix + fakeLevel);
    }

    public AudioClip GetBackgroundMusic(int worldIndex, AudioClip defaultClip)
    {
        AudioClip result = defaultClip;
        WorldLevelConfig w = GetWorldByIndex(worldIndex);
        if(w.musics.ingameMusic != null)
            result = w.musics.ingameMusic;
        return result;
    }

    public AudioClip GetHomeMusicByWorld(int worldIndex, AudioClip defaultClip)
    {
        AudioClip result = defaultClip;
        WorldLevelConfig w = GetWorldByIndex(worldIndex);
        if (w.musics.ingameMusic != null)
            result = w.musics.homeMusic;
        return result;
    }

    public AudioClip GetBossMusicByMode(int worldIndex, int levelIndex, AudioClip defaultClip, PlayMode playMode)
    {
        AudioClip result = defaultClip;
        switch (playMode)
        {
            case PlayMode.Normal:
                WorldLevelConfig w = GetWorldByIndex(worldIndex);
                if (w.musics.ingameMusic != null)
                    result = w.musics.bossFightMusic;
                break;
            case PlayMode.Boss:
                BossConfig boss = config.bossLevels[levelIndex];
                result = boss.bossMusic;
                break;
        }
        return result;
    }
}

public class MapInfo
{
    public int world;
    public int level;
}

public class BossInfo
{
    public string bossName;
    public int unlockLevel;
    public Sprite avatar;
}
