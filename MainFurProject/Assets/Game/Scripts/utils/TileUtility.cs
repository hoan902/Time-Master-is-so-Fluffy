#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileConverType
{
    ToRule,
    ToTile
}

public class TileUtility : MonoBehaviour
{
    public TileConverType convertType = TileConverType.ToRule;
    public RuleTile toRule;
    public List<TileBase> usedTiles = new List<TileBase>();
    public List<TileBase> replaceTiles = new List<TileBase>();

    public void Load()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        usedTiles = new List<TileBase>();
        replaceTiles = new List<TileBase>();
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
            {
                TileBase tile = allTiles[position.x - bounds.xMin + (position.y - bounds.yMin) * bounds.size.x];
                if (tile == null)
                    continue;
                if (!usedTiles.Contains(tile) && !string.IsNullOrEmpty(tile.name))
                {
                    usedTiles.Add(tile);
                    replaceTiles.Add(null);
                }
            }
        }
    }

    public void Convert()
    {
        Tilemap t = GetComponent<Tilemap>();
        for(int i = 0; i < usedTiles.Count; i++)
        {
            switch(convertType)
            {
                case TileConverType.ToRule:
                    t.SwapTile(usedTiles[i], toRule);
                    break;
                case TileConverType.ToTile:
                    if(replaceTiles[i] != null)
                        t.SwapTile(usedTiles[i], replaceTiles[i]);
                    break;
            }
        }
    }
}

[CustomEditor(typeof(TileUtility))]
public class TileUtilityEditor : Editor
{
    private SerializedProperty m_convertType;
    private SerializedProperty m_toRule;
    private SerializedProperty m_replaceTiles;

    private TileUtility m_target;

    private void OnEnable()
    {
        m_convertType = serializedObject.FindProperty("convertType");
        m_toRule = serializedObject.FindProperty ("toRule");
        m_replaceTiles = serializedObject.FindProperty("replaceTiles");

        m_target = (TileUtility)target;
    }

    override public void OnInspectorGUI()
    {        
        EditorGUILayout.Space(20);
        if (GUILayout.Button("Load Data"))
        {
            m_target.Load();
            serializedObject.ApplyModifiedProperties();
        }

        serializedObject.Update();
        if (m_target.usedTiles.Count > 0)
        {

            EditorGUILayout.Space(20);
            EditorGUILayout.PropertyField(m_convertType);
            EditorGUILayout.Space(10);
            if (m_target.convertType == TileConverType.ToRule)
            {
                if (m_toRule.objectReferenceValue == null)
                    EditorGUILayout.PropertyField(m_toRule);
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("To Rule", GUILayout.Height(50), GUILayout.Width(120));
                    GUILayout.Label("", GUILayout.Height(50), GUILayout.Width(50));
                    GUI.DrawTexture(GUILayoutUtility.GetLastRect(), AssetPreview.GetAssetPreview(m_target.toRule.m_DefaultSprite));
                    GUILayout.Label(m_target.toRule.name, GUILayout.Height(50), GUILayout.Width(120));
                    if (GUILayout.Button("Clear"))
                        m_target.toRule = null;
                    EditorGUILayout.EndHorizontal();
                }
            }

            GUILayout.Label("====================================", GUILayout.Height(20), GUILayout.Width(500));
            GUILayout.Label("Tiles:", GUILayout.Height(20), GUILayout.Width(200));

            for (int i = 0; i < m_target.usedTiles.Count; i++)
            {
                TileBase tile = m_target.usedTiles[i];
                Sprite sprite = null;
                if (tile is Tile t)
                    sprite = t.sprite;
                else if (tile is RuleTile r)
                {
                    sprite = r.m_DefaultSprite;
                    m_target.convertType = TileConverType.ToRule;
                }
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(tile.name, GUILayout.Height(50), GUILayout.Width(120));
                GUILayout.Label("", GUILayout.Height(50), GUILayout.Width(50));
                GUI.DrawTexture(GUILayoutUtility.GetLastRect(), AssetPreview.GetAssetPreview(sprite));
                if (m_target.convertType == TileConverType.ToTile && m_target.replaceTiles != null && m_target.replaceTiles.Count > i)
                {
                    TileBase replace = m_target.replaceTiles[i];
                    if (replace == null)
                        EditorGUILayout.PropertyField(m_replaceTiles.GetArrayElementAtIndex(i), new GUIContent(""), GUILayout.Width(120));
                    else
                    {
                        GUILayout.Label(replace.name, GUILayout.Height(50), GUILayout.Width(120));
                        GUILayout.Label("", GUILayout.Height(50), GUILayout.Width(50));
                        if (replace is RuleTile ru)
                            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), AssetPreview.GetAssetPreview(ru.m_DefaultSprite));
                        else if (replace is Tile ti)
                            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), AssetPreview.GetAssetPreview(ti.sprite));
                        if (GUILayout.Button("Clear"))
                            m_target.replaceTiles[i] = null;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(20);
            if (GUILayout.Button("Convert"))
                m_target.Convert();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
