using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ObjectLampDummy : MonoBehaviour
{
    private Light2D m_light;

    private void Awake() 
    {
        m_light = GetComponent<Light2D>();
        if(m_light.pointLightOuterAngle > 350)
        {
            m_light.pointLightOuterAngle = 360;
            m_light.pointLightInnerAngle = 360;
        }
    }

}
