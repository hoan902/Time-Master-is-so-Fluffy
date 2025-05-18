using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectWeather : MonoBehaviour
{
    [SerializeField] private Weather m_weather;
    [SerializeField] private ParticleSystem m_rainEff;

    [SerializeField] private float m_radius = 100f;
    [SerializeField] private float m_rate = 3000;


    private void Start() 
    {
        switch(m_weather)
        {
            case Weather.Rain:
                GameController.UpdateWeather(m_weather);
                m_rainEff.transform.parent.gameObject.SetActive(true);
                m_rainEff.Play();
                break;
            default:
                break;
        }
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(m_radius, 10, 1));    
    }

    public void UpdateSize()
    {
        if(m_weather == Weather.Rain)
        {
            var particle = m_rainEff.shape;
            particle.radius = m_radius / 2;
        }
    }
    public void UpdateRate()
    {
        if(m_weather == Weather.Rain)
        {
            var particle = m_rainEff.emission;
            particle.rateOverTime = m_rate;
        }
    }
}
