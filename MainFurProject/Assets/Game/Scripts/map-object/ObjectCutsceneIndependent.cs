using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ObjectCutsceneIndependent : MonoBehaviour
{
    [SerializeField] private string m_triggerKey = "";

    private bool m_played = false;
    private PlayableDirector m_playable;

    private void Start() 
    {
        m_playable = GetComponent<PlayableDirector>();

        GameController.triggerEvent += OnTrigger;  
    }
    private void OnDestroy() 
    {
        GameController.triggerEvent -= OnTrigger;    
    }

    void OnTrigger(string key, bool state, GameObject triggerSource)
    {
        if (m_triggerKey != key || !state || m_played)
            return;
        m_played = true;
        m_playable.Play();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.tag != GameTag.PLAYER)
            return;    
        if (m_played)
            return;
        m_played = true;
        m_playable.Play();
    }
}
