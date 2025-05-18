using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Tilemaps;

public class STObjectTileHidden : MonoBehaviour
{
    [SerializeField] private Tilemap m_tile;

    private object m_tweener;

    void OnDestroy()
    {
        DOTween.Kill(m_tweener);
    }
    
    public void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.tag != GameTag.PLAYER)
            return;
        STPlayerController player = collider.gameObject.GetComponent<STPlayerController>();
        if(player == null)
            return;
        DOTween.Kill(m_tweener);
        float alpha = m_tile.color.a;
        Color color = Color.white;
        m_tweener = DOTween.To(()=>alpha,  x => alpha = x, 0, 1).OnUpdate(()=>{
            color.a = alpha;
            m_tile.color = color;
        });
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        if(collider.gameObject.tag != GameTag.PLAYER)
            return;
        STPlayerController player = collider.gameObject.GetComponent<STPlayerController>();
        if(player == null)
            return;
        DOTween.Kill(m_tweener);
        float alpha = m_tile.color.a;
        Color color = Color.white;
        m_tweener = DOTween.To(()=>alpha,  x => alpha = x, 1, 1).OnUpdate(()=>{
            color.a = alpha;
            m_tile.color = color;
        });
    }
}
