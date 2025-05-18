using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindySword : Weapon
{
    [SerializeField] private Transform m_shotPoint;
    [SerializeField] private GameObject m_wind;
    [SerializeField] private float m_windSpeed = 10;
    [SerializeField] private float m_windLifeTime = 3;
    [SerializeField] private int m_windDamage = 10;
    [SerializeField] private float m_delayWindHit = 0.1f;
    [SerializeField] private AudioClip m_audioWind;
    
    public override void StartHit()
    {
        canHit = true;
        ActiveSkill();
    }

    void ActiveSkill()
    {
        DamageDealerInfo data = attackData;
        StartCoroutine(IBaseHit(data));
        Shot(data);
    }

    void Shot(DamageDealerInfo data)
    {
        GameObject wind = Instantiate(m_wind, m_shotPoint.position, Quaternion.identity, player.parent);
        wind.SetActive(true);
        int direction = (int)Mathf.Sign(player.transform.localScale.x);
        wind.GetComponent<Wind>().Init(m_windDamage, direction, m_windSpeed, m_windLifeTime, m_delayWindHit);
        SoundManager.PlaySound(m_audioWind, false);
    }
    IEnumerator IBaseHit(DamageDealerInfo data)
    {
        List<Collider2D> hited = new List<Collider2D>();
        float startTime = Time.realtimeSinceStartup;
        bool sent = false;
        bool wallSent = false;
        bool monsterHited = false;

        while(canHit)
        {
            List<Collider2D> results = new List<Collider2D>();
            //enemy check
            enemyAttackArea.OverlapCollider(enemyContactFilter, results);
            for (int i = 0; i < results.Count; i++)
            {
                if(hited.Contains(results[i]))
                    continue;
                if(results[i].GetComponent<STObjectInteractive>() == null)
                    continue;
                hited.Add(results[i]);
                if(results[i].GetComponent<STObjectMonster>())
                    monsterHited = true;
                results[i].SendMessage(EVENT_HIT, data, SendMessageOptions.DontRequireReceiver);
            }
            //wall check
            if (!wallSent)
            {
                wallAttackArea.OverlapCollider(wallContactFilter, results);
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].CompareTag(GameTag.GROUND) || results[i].CompareTag(GameTag.WALL))
                    {
                        data.attacker.SendMessage(EVENT_BEAT_WALL);
                        data.attacker.SendMessage(EVENT_WALL_KNOCKBACK);
                        wallSent = true;
                        break;
                    }
                }
            }

            yield return new WaitForEndOfFrame();
            if (!sent && hited.Count > 0)
            {
                sent = true;
                if (data.critical)
                {
                    data.attacker.SendMessage(EVENT_BEAT_CRITICAL);
                    GameController.ShakeCamera();
                }
                else
                {
                    data.attacker.SendMessage(EVENT_BEAT_NORMAL);
                    GameController.ShakeCameraWeak();
                }
            }
            if (data.critical && hited.Count > 0 && monsterHited)
            {
                if (Time.realtimeSinceStartup - startTime > criticalTimeFreeze)
                    Time.timeScale = 1;
                else
                    Time.timeScale = 0;
            }
        }

        Time.timeScale = 1;
    }
}
