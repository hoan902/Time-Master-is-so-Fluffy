using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGoldPotionChild : MonoBehaviour
{
    [SerializeField] AudioClip m_audioTransform;

    private List<GameObject> m_sounds;

    private void Awake() 
    {
        m_sounds = new List<GameObject>();
    }
    private void OnDestroy() 
    {
        if(m_sounds.Count == 0)
            return;
        foreach(GameObject sound in m_sounds)
        {
            Destroy(sound);
        }    
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        ObjectBase parentBase = other.GetComponentInParent<ObjectBase>();
        if(!parentBase)
            return;
        if(parentBase.Transform())
        {
            PlaySound();
            Destroy(parentBase.gameObject);
        }
    }

    void PlaySound()
    {
        GameObject go = SoundManager.PlaySound3D(m_audioTransform, 5, false, transform.position);
        m_sounds.Add(go);
    }

}
