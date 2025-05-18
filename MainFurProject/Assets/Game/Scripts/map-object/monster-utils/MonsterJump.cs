using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterJump : MonoBehaviour
{
    private Rigidbody2D m_rigidbody;

    public void OnJump(float jumpValue)
    {
        if (m_rigidbody == null)
            return;
        Vector2 veloc = m_rigidbody.velocity;
        veloc.y = jumpValue;
        m_rigidbody.velocity = veloc;
    }
}
