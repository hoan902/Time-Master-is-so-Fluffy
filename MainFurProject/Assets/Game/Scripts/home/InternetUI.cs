using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternetUI : MonoBehaviour
{
    [SerializeField] private GameObject m_panel;

    IEnumerator Start()
    {
        while (true)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                /*if (RemoteConfig.showNoInternet && MainModel.readyToCheckInternet)
                {
                    Time.timeScale = 0;
                    m_panel.SetActive(true);
                }*/
            }
            else
            {
                if (m_panel.activeSelf)
                {
                    if (!MainModel.paused)
                        Time.timeScale = 1;
                    m_panel.SetActive(false);
                }
            }
            yield return new WaitForSecondsRealtime(5f);
        }
    }
}
