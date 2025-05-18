using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSkinSelection : MonoBehaviour
{
    [SerializeField] private GameObject m_skinItem;
    [SerializeField] private Transform m_itemContainer;

    private List<SkinSelectionItem> m_items;

    public void Init()
    {
        m_items = new List<SkinSelectionItem>();
        foreach(string skin in MainModel.unlockedSkins)
        {
            GameObject item = Instantiate(m_skinItem, m_itemContainer);
            item.SetActive(true);
            SkinSelectionItem itemComp = item.GetComponent<SkinSelectionItem>();
            itemComp.Init(skin);
            item.GetComponent<Button>().onClick.AddListener(() => ItemOnClick(skin));
            m_items.Add(itemComp);
        }

        ItemOnClick(MainModel.currentSkin);
    }

    void ItemOnClick(string skin)
    {
        PopupSkinComboSelection.skinOnClickEvent?.Invoke(skin);
    }
}
