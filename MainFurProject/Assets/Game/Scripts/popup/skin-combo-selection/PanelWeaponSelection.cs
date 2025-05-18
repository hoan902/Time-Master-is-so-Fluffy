using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelWeaponSelection : MonoBehaviour
{
    [SerializeField] private GameObject m_skinItem;
    [SerializeField] private Transform m_itemContainer;

    private List<WeaponSelectionItem> m_items;

    public void Init()
    {
        m_items = new List<WeaponSelectionItem>();
        foreach (string weaponSkinName in MainModel.unlockedWeapons)
        {
            GameObject item = Instantiate(m_skinItem, m_itemContainer);
            item.SetActive(true);
            WeaponSelectionItem itemComp = item.GetComponent<WeaponSelectionItem>();
            itemComp.Init(weaponSkinName);
            item.GetComponent<Button>().onClick.AddListener(() => ItemOnClick(weaponSkinName));
            m_items.Add(itemComp);
        }

        ItemOnClick(MainModel.currentWeapon);
    }

    void ItemOnClick(string weaponSkinName)
    {
        PopupSkinComboSelection.weaponOnClickEvent?.Invoke(weaponSkinName);
    }
}
