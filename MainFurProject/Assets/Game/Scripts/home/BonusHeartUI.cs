using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusHeartUI : MonoBehaviour
{
    [SerializeField] private GameObject m_bonusHeart;
    [SerializeField] private GameObject m_layerEffect;
    [SerializeField] private AudioClip m_audioBonus;//collect-item
    [SerializeField] private AudioClip m_audioCollect;//bonus-heart

    void Awake()
    {
        MainController.bonusHeartEvent += OnBonusHeart;
    }

    private void OnDestroy()
    {
        MainController.bonusHeartEvent -= OnBonusHeart;
    }

    private void OnBonusHeart(int heart)
    {
        SoundManager.PlaySound(m_audioBonus, false);
        m_bonusHeart.SetActive(true);
        m_bonusHeart.transform.Find("text-heart-value").GetComponent<TextMeshProUGUI>().text = "+" + heart;
        StartCoroutine(BonusHeart(heart));
    }

    IEnumerator BonusHeart(int heart)
    {
        int startHeart = MainModel.totalHeart - heart;
        yield return new WaitForSeconds(0.2f);
        GameObject icon = m_bonusHeart.transform.Find("icon").gameObject;
        Vector3 pos1 = icon.transform.position;
        Vector3 pos2 = pos1 + new Vector3(1, 1);
        Vector3 pos3 = MainScene.m_avatarObject.position;
        Vector3[] path = new Vector3[] { pos1, pos2, pos3 };
        for (int i = 0; i < heart; i++)
        {
            GameObject go = Instantiate(icon);
            go.transform.SetParent(m_layerEffect.transform);
            go.transform.position = icon.transform.position;
            var i1 = i;
            SoundManager.PlaySound(m_audioCollect, false);
            go.transform.localScale = new Vector3(2f, 2f, 1);
            Image img = go.GetComponent<Image>();
            Color color = img.color;
            color.a = 0;
            img.color = color;
            img.DOFade(1, 0.2f);
            go.transform.DOScale(Vector3.one, 0.2f).OnComplete(() => {
                go.transform.DOPath(path, 0.8f, PathType.CatmullRom).SetEase(Ease.InCubic).OnComplete(() => {
                    go.transform.DOScale(new Vector3(1.5f, 1.5f, 1), 0.5f);
                    img.DOFade(0, 0.5f).OnComplete(() => {
                        MainController.SimulateUpdateHeart(startHeart + i1 + 1);
                        Destroy(go);
                    });
                });
            });
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(1f);
        m_bonusHeart.SetActive(false);
    }
}
