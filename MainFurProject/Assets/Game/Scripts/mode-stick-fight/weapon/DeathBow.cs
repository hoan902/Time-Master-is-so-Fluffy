using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeathBow : Weapon
{
    [SerializeField] private Transform m_shotPoint;
    [SerializeField] private GameObject m_arrow;
    [SerializeField] private float m_arrowSpeed = 5;
    [SerializeField] private AudioClip m_audioShot;
    [SerializeField] private float m_aimingAngle = 30;
    [SerializeField] private LayerMask m_monsterLayer;

    private readonly Vector2 DETECT_MONSTER_OFFSET = new Vector2(0.5f, 1);
    private readonly Vector2 DETECT_MONSTER_SIZE = new Vector2(1, 10);
    
    public override void StartHit()
    {
        canHit = true;
        ActiveSkill();
    }

    void ActiveSkill()
    {
        GameController.VibrateCustom(new Vector3(0.1f, 0, 0), 0.1f);
        Shot();
    }

    void Shot()
    {
        Vector2 playerFaceDirection = (int)Mathf.Sign(player.transform.localScale.x) > 0 ? Vector2.right : Vector2.left;
        Vector2 boxOrigin = transform.position + (Vector3)DETECT_MONSTER_OFFSET;
        RaycastHit2D hit = Physics2D.BoxCast(boxOrigin, DETECT_MONSTER_SIZE, 0, playerFaceDirection, 15f, m_monsterLayer);
        Vector2 shootDirection = playerFaceDirection;
        if (hit.collider != null && hit.collider.GetComponent<STObjectInteractive>() && !hit.collider.GetComponent<STObjectInteractive>().isDead)
        {
            float angleToTarget = Vector3.SignedAngle(playerFaceDirection, (hit.collider.transform.position - transform.position), Vector3.forward);
            if(Mathf.Abs(angleToTarget) <= m_aimingAngle)
            {
                shootDirection = (hit.point - (Vector2)m_shotPoint.position).normalized;
            }
        }

        GameObject arrow = Instantiate(m_arrow, m_shotPoint.position, Quaternion.identity, player.parent);
        arrow.SetActive(true);
        arrow.GetComponent<Arrow>().Init(attackData.damage, shootDirection, m_arrowSpeed);
        SoundManager.PlaySound(m_audioShot, false);
    }
    
    public override void Freeze()
    {
        stopMoveAtBegin = true;
        canFreezeFight = false;
        StartCoroutine(IIgnoreFall(freezeTime));
    }
    
    IEnumerator IIgnoreFall(float duration)
    {
        Rigidbody2D m_physicBody = player.GetComponent<Rigidbody2D>();
        float start = Time.time;
        m_physicBody.velocity = Vector2.zero;
        float startPosY = m_physicBody.position.y;
        while ((Time.time - start) < duration)
        {
            yield return new WaitForFixedUpdate();
            Vector2 velocity = m_physicBody.velocity;
            if(velocity.y < 0)
                velocity.y = 0;
            m_physicBody.velocity = velocity;
            if (m_physicBody.position.y < startPosY)
                m_physicBody.position = new Vector2(m_physicBody.position.x, startPosY);
        }
        stopMoveAtBegin = false;
    }
}
