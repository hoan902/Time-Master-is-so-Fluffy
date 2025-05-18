using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class ObjectChest : MonoBehaviour
{
    public int coins = 100;
    public float timeDelay = 0.2f;

    [SerializeField] private Transform m_center;
    [SerializeField] private GameObject m_coinEffect;
    [SerializeField] private SkeletonAnimation m_spine;
    [SerializeField] private AudioClip m_audioOpen;
    [SerializeField] private AudioClip m_audioCollect;//collect-coin

    private bool m_opened = false;

    public void OnTriggerEnter2D(Collider2D collider)
    {
        if(m_opened || collider.tag != GameTag.WEAPON)
            return;
        m_opened = true;
        GameController.UpdateCoin(coins);
        SoundManager.PlaySound3D(m_audioOpen, 10, false, transform.position);
        HurtFlashEffect eff = m_spine.GetComponent<HurtFlashEffect>();
        if(eff != null)
            eff.Flash();
        m_spine.AnimationState.SetAnimation(0, "open", false);
        StartCoroutine(ICoins());
    }

    IEnumerator ICoins()
    {
        int coinValue = coins*MapConstant.COIN_RATIO;
        int count = coinValue/100;
        int last = coinValue%100;
        int total = count + (last < 1 ? 0 : 1);
        while(total > 0)
        {
            SoundManager.PlaySound(m_audioCollect, false);
            GameObject eff = Instantiate(m_coinEffect);
            eff.transform.SetParent(transform.parent, false);
            eff.transform.position = m_center.position;
            eff.SetActive(true);
            int valCoin = count < 1 ? last : 100;
            eff.GetComponent<CoinSingleEffect>().Init(valCoin/MapConstant.COIN_RATIO, m_coinEffect.transform.position, transform.parent);
            total--;
            count--;
            yield return new WaitForSeconds(timeDelay);
        }
        Destroy(gameObject);
    }
}
