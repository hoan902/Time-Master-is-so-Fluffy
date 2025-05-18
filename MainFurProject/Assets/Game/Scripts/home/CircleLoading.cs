
using System.Collections;
using UnityEngine;

public class CircleLoading : MonoBehaviour
{
    [SerializeField] private GameObject m_fader;

    public void Awake()
    {
        MainController.activeLoadingEvent += OnActive;
    }

    void OnDestroy()
    {
        MainController.activeLoadingEvent -= OnActive;
        StopAllCoroutines();
    }

    private void OnActive(bool active, int childIndex)
    {
        m_fader.SetActive(active);
        for(int i = 0; i < m_fader.transform.childCount; i++)
        {
            m_fader.transform.GetChild(i).gameObject.SetActive(i == childIndex);
        }
        StopAllCoroutines();
        if(active)
            StartCoroutine(WaitStop());
    }

    IEnumerator WaitStop()
    {
        yield return new WaitForSeconds(5f);
        m_fader.SetActive(false);
    }
}
