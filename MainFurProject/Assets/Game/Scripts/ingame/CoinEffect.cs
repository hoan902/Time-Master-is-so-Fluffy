using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CoinEffect : MonoBehaviour
{
    public float timeDelay = 0.2f;
    [SerializeField] private GameObject m_coinEffect;
    [SerializeField] private AudioClip m_audioCollect;//collect-coin

    public void Init(int coins, Transform parent)
    {
        if(coins < 1)
        {            
            Destroy(gameObject);
            return;
        }    
        gameObject.SetActive(true);
        gameObject.transform.SetParent(parent, true);
        StartCoroutine(ICoins(coins, parent));
        GameController.UpdatePointMonster(coins);
    }

    IEnumerator ICoins(int coins, Transform parent)
    {
        int coinValue = coins * MapConstant.COIN_RATIO;
        int count = coinValue / 100;
        int last = coinValue % 100;
        int total = count + (last < 1 ? 0 : 1);
        while (total > 0)
        {
            SoundManager.PlaySound(m_audioCollect, false);
            GameObject eff = Instantiate(m_coinEffect);
            eff.SetActive(true);            
            int valCoin = count < 1 ? last : 100;
            eff.GetComponent<CoinSingleEffect>().Init(valCoin / MapConstant.COIN_RATIO, transform.position, parent);            
            total--;
            count--;
            yield return new WaitForSeconds(timeDelay);
        }
        Destroy(gameObject);
    }
}
