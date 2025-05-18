using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusCoinUI : MonoBehaviour
{

    [SerializeField] private GameObject m_bonusCoin;
    [SerializeField] private GameObject m_layerEffect;
    [SerializeField] private AudioClip m_audioCollectCoin;//collect-coin
    [SerializeField] private AudioClip m_audioBonus;//collect-item
    [SerializeField] private AudioClip m_audioRainCoin;//rain-coin

    // Start is called before the first frame update
    void Awake()
    {
        MainController.bonusCoinEvent += OnBonusCoin;
    }

    private void OnDestroy()
    {
        MainController.bonusCoinEvent -= OnBonusCoin;
    }

    private void OnBonusCoin(int coin)
    {
        SoundManager.PlaySound(m_audioBonus, false);
        m_bonusCoin.SetActive(true);
        m_bonusCoin.transform.Find("text-coin-value").GetComponent<TextMeshProUGUI>().text = "+" + coin;
        StartCoroutine(BonusCoin(coin));
    }

    IEnumerator BonusCoin(int coin)
    {
        yield return new WaitForSeconds(0.5f);
        SoundManager.PlaySound(m_audioRainCoin, false);
        GameObject icon = m_bonusCoin.transform.Find("icon").gameObject;
        //OnUpdateCoin(MainModel.totalCoin);
        Vector3 pos1 = icon.transform.position;
        Vector3 pos2 = pos1 + new Vector3(-1, 1);
        Vector3 pos3 = MainScene.m_coinObject.position;
        Vector3[] path = new Vector3[] { pos1, pos2, pos3 };
        int last = MainModel.totalCoin - coin;
        int count = Mathf.CeilToInt(coin * 1f / MapConstant.COIN_RATIO);
        int max = Mathf.Min(10, count);
        float ratio = coin * 1f / max;
        for (int i = 0; i < max; i++)
        {
            GameObject go = Instantiate(icon, m_layerEffect.transform);
            go.SetActive(true);
            go.transform.position = icon.transform.position;
            go.transform.localScale = new Vector3(3f, 3f, 1);
            Image img = go.GetComponent<Image>();
            Color color = img.color;
            color.a = 0;
            img.color = color;
            img.DOFade(1, 0.2f);
            go.transform.DOScale(new Vector3(1f, 1f, 1), 0.2f).OnComplete(() => {
                go.transform.DOPath(path, 0.8f, PathType.CatmullRom).SetEase(Ease.InCubic).OnComplete(() => {
                    SoundManager.PlaySound(m_audioCollectCoin, false);
                    go.transform.DOScale(new Vector3(3, 3, 1), 0.5f);
                    img.DOFade(0, 0.5f).OnComplete(() => {
                        int total = last + Mathf.RoundToInt(Mathf.Min((i + 1) * ratio, coin));
                        MainController.SimulateUpdateCoin(total);
                        MainController.UpdateFakeCoin(total);
                        Destroy(go);
                    });
                });
            });
            yield return new WaitForSeconds(0.1f);
        }
        //m_bonusCoin.GetComponent<Animation>().Play("BonusCoin");
        yield return new WaitForSeconds(1f);
        m_bonusCoin.SetActive(false);
        //Destroy(sound);
    }
}
