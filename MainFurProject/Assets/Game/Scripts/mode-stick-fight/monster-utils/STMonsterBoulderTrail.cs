using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class STMonsterBoulderTrail : MonoBehaviour
{
    private STEnemyPhysicBoulder m_boulder;
    private float m_offsetY = -0.5f;
    private ParticleSystem m_particle;
    private TrailRenderer m_trailRenderer;

    public void Init(STEnemyPhysicBoulder bullet)
    {
        m_trailRenderer = GetComponentInChildren<TrailRenderer>();
        m_particle = GetComponent<ParticleSystem>();
        m_boulder = bullet;
    }

    private void Update() 
    {
        if(m_boulder == null)
            return;
        if(!m_boulder.isGrounded)
        {
            m_particle.Stop();
            if(m_trailRenderer)
                m_trailRenderer.enabled = false;
            return;
        }
        else if(!m_particle.isPlaying)
        {
            m_particle.Play();
            if(m_trailRenderer)
                m_trailRenderer.enabled = true;
        }
        Vector3 boulderPos = m_boulder.bottomPos.position;
        transform.position = new Vector3(boulderPos.x, boulderPos.y + m_offsetY, boulderPos.z);
    }
}
