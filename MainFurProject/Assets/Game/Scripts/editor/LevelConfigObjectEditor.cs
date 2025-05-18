using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameConfigObject))]
public class LevelConfigObjectEditor : Editor
{
    private GameConfigObject m_target;

    private void OnEnable()
    {
        m_target = target as GameConfigObject;
        UpdateAssets();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Refresh"))
        {
            UpdateAssets();            
        }
    }

    void UpdateAssets()
    {
        UpdateBossAvatar();
        UpdatePlayerAvatar();
        EditorUtility.SetDirty(m_target);
    }

    void UpdateBossAvatar()
    {
        string fullPath = "Assets/Game/Assets/Textures/UI/boss";
        if (!Directory.Exists(fullPath))
        {
            return;
        }
        string[] guids = AssetDatabase.FindAssets("t:sprite", new string[] { fullPath });
        List<Sprite> avatars = new List<Sprite>();
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            avatars.Add(sprite);
        }
        m_target.UpdateBossAvatars(avatars);
    }

    void UpdatePlayerAvatar()
    {
        string fullPath = "Assets/Game/Assets/Textures/UI/avatars";
        if (!Directory.Exists(fullPath))
        {
            return;
        }
        string[] guids = AssetDatabase.FindAssets("t:sprite", new string[] { fullPath });
        List<Sprite> avatars = new List<Sprite>();
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            avatars.Add(sprite);
        }
        m_target.UpdatePlayerAvatars(avatars);
    }
}
