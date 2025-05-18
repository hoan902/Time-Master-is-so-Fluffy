using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneSkin : MonoBehaviour
{
    [Range(1, 150)]
    [SerializeField] private int m_skin = 1;
    [SerializeField] private bool m_isTry;

    public void OnSkin()
    {
        if (m_isTry)
        {
            // MainController.TrySkin(m_skin.ToString());
        }
        else
        {
            // MainController.BuyCoinSkin(0, m_skin.ToString());
            // MainController.SelectSkin(m_skin.ToString());
        }
    }
}
