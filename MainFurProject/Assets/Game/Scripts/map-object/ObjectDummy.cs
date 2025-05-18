using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDummy : MonoBehaviour
{
    public GameObject dummyObject;

    private bool m_isQuitting = false;

    void Awake()
    {
        GameController.quitEvent += OnQuit;
    }

    void OnDestroy()
    {
        GameController.quitEvent -= OnQuit;
        if(m_isQuitting)
            return;
        GameObject go = Instantiate(dummyObject);
        go.transform.SetParent(transform.parent, false);
        go.transform.position = transform.position;
    }

    private void OnQuit(QuitGameReason obj, bool force)
    {
        m_isQuitting = true;
    }

    void OnApplicationQuit()
    {
        m_isQuitting = true;
    }
}
