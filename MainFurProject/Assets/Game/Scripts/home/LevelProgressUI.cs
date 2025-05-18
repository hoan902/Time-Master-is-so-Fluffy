using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelProgressUI : MonoBehaviour
{
    [SerializeField] private GameObject m_content;
    [SerializeField] private CanvasGroup m_canvas;
    [SerializeField] private Transform m_endPos;
    [SerializeField] private Image m_bossAvatar;
    [SerializeField] private Sprite[] m_spritesYellow;
    [SerializeField] private Sprite[] m_spritesGreen;

    private bool m_stop;
    private GamePlayScene m_gameplayScene;

    void Awake()
    {
        m_gameplayScene = GetComponentInParent<GamePlayScene>();

        GameController.showProgressLevelEvent += OnShow;
        InputController.leftAction += OnInput;
        InputController.rightAction += OnInput;
        InputController.jumpAction += OnInput;
    }

    private void OnDestroy()
    {
        GameController.showProgressLevelEvent -= OnShow;
        InputController.leftAction -= OnInput;
        InputController.rightAction -= OnInput;
        InputController.jumpAction -= OnInput;
    }

    private void OnShow()
    {
        m_content.SetActive(true);
        Init();
        DoEffect();
    }

    private void OnInput()
    {
        if (m_stop)
            return;
        m_stop = true;
        m_canvas.DOFade(0, 0.25f).OnComplete(() => {
            m_content.SetActive(false);
            // if(!m_gameplayScene.IsBossLevel)
            //     m_gameplayScene.ShowPointObject();
        }).SetDelay(3f);
    }

    void Init()
    {
        List<WorldLevelConfig> worlds = ConfigLoader.instance.worlds;
        int worldIndex = MainModel.gameInfo.world % worlds.Count;
        int mapLevel = MainModel.gameInfo.level;
        string nameNode = "node-";
        MapInfo nextBoss = ConfigLoader.instance.GetNextBossLevel(worldIndex, mapLevel);
        MapInfo begin = ConfigLoader.instance.GetBeginLevelProgress(worldIndex, mapLevel);
        
        if (nextBoss == null || begin == null)
        {
            m_content.SetActive(false);
            return;
        }
        string bossName = ConfigLoader.GetBossName(ConfigLoader.instance.GetLevel(nextBoss.world, nextBoss.level).levelPath);
        m_bossAvatar.sprite = ConfigLoader.instance.FindBossAvatar(bossName);
        m_bossAvatar.SetNativeSize();
        GameObject nodes = m_content.transform.Find("nodes").gameObject;
        GameObject nodeNormal = nodes.transform.Find("node-normal").gameObject;
        GameObject nodeBonus = nodes.transform.Find("node-bonus").gameObject;
        GameObject nodeSkin = nodes.transform.Find("node-skin").gameObject;

        bool stop = false;
        bool foundCurrent = false;
        int counter = 1000;
        int worldStart = begin.world;
        int levelStart = begin.level;
        while (!stop && counter > 0)
        {
            for (int i = worldStart; i < worlds.Count; i++)
            {
                WorldLevelConfig world = ConfigLoader.instance.worlds[i];
                for (int j = levelStart; j < world.levels.Count; j++)
                {
                    string levelName = world.levels[j].levelPath;
                    GameObject go;
                    int spriteIndex;
                    if (ConfigLoader.IsBonusLevel(levelName))
                    {
                        go = Instantiate(nodeBonus);
                        spriteIndex = 0;
                    }
                    else if (ConfigLoader.IsSkinLevel(levelName))
                    {
                        go = Instantiate(nodeSkin);
                        spriteIndex = 0;
                    }
                    else
                    {
                        go = Instantiate(nodeNormal);
                        spriteIndex = 1;
                    }
                    go.name = nameNode + levelName;
                    go.SetActive(true);
                    go.transform.SetParent(nodes.transform, false);
                    Transform icon = go.transform.Find("icon");
                    bool current = i == worldIndex && j == mapLevel;                    
                    icon.gameObject.SetActive(!foundCurrent);
                    if (current)
                        foundCurrent = true;
                    icon.GetComponent<Image>().sprite = current ? m_spritesYellow[spriteIndex] : m_spritesGreen[spriteIndex];
                    if (i == nextBoss.world && j == nextBoss.level)
                    {
                        stop = true;
                        break;
                    }
                }
                levelStart = 0;
                if (stop)
                    break;
            }
            worldStart = 0;
            counter--;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(nodes.GetComponent<RectTransform>());
    }

    void DoEffect()
    {
        m_canvas.alpha = 1;
        m_content.transform.localScale = Vector3.zero;
        m_content.transform.localPosition = Vector3.zero;
        m_content.transform.DOScale(Vector3.one, 0.25f).OnComplete(()=> {
            m_content.transform.DOLocalMoveY(m_endPos.localPosition.y, 0.5f).SetDelay(1f);
        });
    }
}
