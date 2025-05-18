using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelBoostItems : MonoBehaviour
{
    [SerializeField] private GameObject m_itemDamage;
    [SerializeField] private GameObject m_itemHealth;

    private BoostDamageItem m_damage;
    private BoostHealthItem m_health;

    private int m_count;

    void Awake()
    {
        GameController.activeBoostItemEvent += OnActiveItem;
        GameController.useBoostItemEvent += OnUseItem;
        //
        m_count = 0;
    }

    void OnDestroy()
    {
        GameController.activeBoostItemEvent -= OnActiveItem;
        GameController.useBoostItemEvent -= OnUseItem;
    }

    private void OnUseItem(object item)
    {
        m_count--;
        GameObject go = null;
        if (item is BoostDamageItem)
        {
            go = m_itemDamage;
        }
        else if (item is BoostHealthItem)
        {
            go = m_itemHealth;
        }

        if (go == null)
            return;
        go.transform.Find("button-buy").gameObject.SetActive(false);
        go.transform.Find("button-get").gameObject.SetActive(false);
        //
        if (m_count < 1)
            StartCoroutine(DelayQuit());
    }

    private void OnActiveItem(object item)
    {
        if (item is BoostDamageItem)
        {
            BoostDamageItem dame = item as BoostDamageItem;
            m_damage = dame;
            m_itemDamage.SetActive(dame.active);
            m_itemDamage.transform.Find("title").GetComponent<TextMeshProUGUI>().text = "X" + dame.ratio + " DAMAGE";
            m_itemDamage.transform.Find("button-buy").GetComponent<Button>().interactable =
                MainModel.totalCoin >= dame.price;
            m_itemDamage.transform.Find("button-buy/text").GetComponent<TextMeshProUGUI>().text =
                GameUtils.CoinToString(dame.price);
            m_count += dame.active ? 1 : 0;
        }
        else if (item is BoostHealthItem)
        {
            BoostHealthItem health = item as BoostHealthItem;
            m_health = health;
            m_itemHealth.SetActive(health.active);
            m_itemHealth.transform.Find("title").GetComponent<TextMeshProUGUI>().text = "X" + health.ratio + " HEALTH";
            m_itemHealth.transform.Find("button-buy").GetComponent<Button>().interactable =
                MainModel.totalCoin >= health.price;
            m_itemHealth.transform.Find("button-buy/text").GetComponent<TextMeshProUGUI>().text =
                GameUtils.CoinToString(health.price);
            m_count += health.active ? 1 : 0;
        }
    }

    public void BuyItemDamageOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if (MainModel.totalCoin < m_damage.price)
            return;
        MainController.UpdateCoin(m_damage.price * -1);
        GameController.UseBoostItem(m_damage);
        //
        m_itemHealth.transform.Find("button-buy").GetComponent<Button>().interactable =
            MainModel.totalCoin >= m_health.price;
    }

    public void GetItemDamageOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if (Application.isEditor)
            GameController.UseBoostItem(m_damage);
        else
            GameController.UseBoostItem(m_damage);
    }

    public void BuyItemHealthOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if (MainModel.totalCoin < m_damage.price)
            return;
        MainController.UpdateCoin(m_health.price * -1);
        GameController.UseBoostItem(m_health);
        //
        m_itemDamage.transform.Find("button-buy").GetComponent<Button>().interactable =
            MainModel.totalCoin >= m_damage.price;
    }

    public void GetItemHealthOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        if (Application.isEditor)
            GameController.UseBoostItem(m_health);
        else
            GameController.UseBoostItem(m_health);
    }

    public void CloseOnclick()
    {
        SoundManager.PlaySound(GameConstant.AUDIO_CLICK, false);
        StopAllCoroutines();
        GameController.BoostItem();
        MainController.ClosePopup(PopupType.BoostItems);
    }

    IEnumerator DelayQuit()
    {
        yield return new WaitForSeconds(0.5f);
        GameController.BoostItem();
        MainController.ClosePopup(PopupType.BoostItems);
    }
}