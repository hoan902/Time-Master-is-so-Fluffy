using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using DG.Tweening;
using TMPro;

public class ObjectSingleCoinPhysics : MonoBehaviour
{
    [SerializeField] private GameObject m_effect;
    [SerializeField] private GameObject m_text;
    [SerializeField] private AudioClip m_audioCollect;

    private bool m_stop = true;
    private Transform m_target;

    private void Start() 
    {
        m_stop = false;    
    }

    public void OnTrigger(Collider2D other)
    {
        if(other.gameObject.tag != GameTag.PLAYER || m_stop)
            return;
        SoundManager.PlaySound(m_audioCollect, false);
        m_stop = true;
        m_effect.transform.parent.SetParent(transform.parent, false);
        m_effect.transform.parent.position = transform.position;
        m_effect.transform.parent.gameObject.SetActive(true);
        m_effect.GetComponent<SkeletonAnimation>().AnimationState.Complete += (entry) =>
        {
            Destroy(m_effect.transform.parent.gameObject);
        };

        //effect text
        TextMeshPro text = m_text.GetComponent<TextMeshPro>();
        text.text = "+" + MapConstant.COIN_RATIO;
        m_text.SetActive(true);
        m_text.transform.SetParent(transform.parent, true);
        m_text.transform.localEulerAngles = Vector3.zero;
        m_text.transform.DOMoveY(transform.position.y + 1.5f, 0.5f).OnComplete(()=>{
            text.DOFade(0, 0.5f).OnComplete(()=>{
                m_text.transform.DOKill();
                text.DOKill();
                Destroy(m_text);
            });
        });

        GameController.UpdateCoin(1);
        Destroy(gameObject);
    }
}
