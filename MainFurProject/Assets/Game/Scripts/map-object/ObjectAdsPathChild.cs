using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAdsPathChild : MonoBehaviour
{
    [SerializeField] private ObjectAdsPath m_parent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag != GameTag.PLAYER)
            return;
        m_parent.MoveToNextPoint();
    }
}
