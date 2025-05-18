using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCloudFan : MonoBehaviour
{
    [SerializeField] private float m_height;
    [SerializeField] private float m_timeMove = 2f;
    public BoxCollider2D boxColidder;
    [SerializeField] private BuoyancyEffector2D m_effector;
    [SerializeField] private SpriteRenderer m_wind;
    public SkeletonAnimation cloud;

    private Material m_textureToAnimate;
    private Vector2 m_uvOffset = Vector2.zero;
    private Vector2 m_uvAnimationRate = new Vector2(0, 2f);

    private bool m_active;

    private void Start()
    {
        cloud.AnimationState.SetAnimation(0, "sleep", true);
        m_textureToAnimate = m_wind.material;        
    }

    private void Update()
    {
        if (!m_active)
            return;
        if (m_textureToAnimate != null)
        {
            if (m_uvOffset.y >= 1.0f)
            {
                m_uvOffset.y = 0.0f;
            }

            m_uvOffset -= m_uvAnimationRate * Time.deltaTime;
            m_textureToAnimate.mainTextureOffset = m_uvOffset;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {        
        if (m_active || collision.tag != GameTag.PLAYER)
            return;
        m_active = true;
        cloud.AnimationState.SetAnimation(0, "wake_up", true);
        Vector2 size = boxColidder.size;
        Vector2 offset = boxColidder.offset;
        Vector3 pos = cloud.transform.position;
        Vector2 spriteSize = m_wind.size;
        float startSize = size.y;
        DOTween.To(() => size.y, x =>
        {
            m_effector.surfaceLevel = x;
            size.y = x;
            offset.y = x / 2;
            boxColidder.size = size;
            boxColidder.offset = offset;
            cloud.transform.position = pos + new Vector3(0, x - startSize);
            spriteSize.y = x/2 - 0.5f;
            m_wind.size = spriteSize;

        }, m_height, m_timeMove).SetEase(Ease.Linear);
    }
}
