using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupBossCollection : MonoBehaviour
{
    [SerializeField] private Transform m_content;
    [SerializeField] private GameObject m_itemSample;

    private List<BossInfo> m_bosses;

    IEnumerator Start()
    {
        m_bosses = ConfigLoader.instance.GetAllBoss();
        int fakeLevelIndex = ConfigLoader.instance.fakeMapLevel;
        for (int i = 0; i < m_bosses.Count; i++)
        {
            BossInfo info = m_bosses[i];
            bool unlock = fakeLevelIndex > info.unlockLevel;
            GameObject go = Instantiate(m_itemSample, m_content, false);
            go.name = info.bossName;
            go.SetActive(true);
            Image icon = go.transform.Find("frame/icon").GetComponent<Image>();
            icon.sprite = info.avatar;
            Rect rect = info.avatar.rect;
            icon.GetComponent<AspectRatioFitter>().aspectRatio = rect.width / rect.height;
            go.transform.Find("icon-lock").gameObject.SetActive(!unlock);
            TextMeshProUGUI text = go.transform.Find("info-text").GetComponent<TextMeshProUGUI>();
            text.text = unlock ? "" : $"unlock at level {info.unlockLevel}";
            go.transform.Find("button-play").gameObject.SetActive(unlock);
            yield return null;
        }
    }

    public void CloseOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        MainController.ClosePopup(PopupType.BossCollection);
    }

    public void ItemOnclick(GameObject go)
    {
        MainController.ClosePopup(PopupType.BossCollection);
        int levelIndex = 0;
        foreach (BossConfig bossConfig in ConfigLoader.instance.config.bossLevels)
        {
            if (bossConfig.bossName == go.name)
                break;
            levelIndex++;
        }

        MainController.PlayGame(-1, levelIndex, PlayMode.Boss);
    }
}
