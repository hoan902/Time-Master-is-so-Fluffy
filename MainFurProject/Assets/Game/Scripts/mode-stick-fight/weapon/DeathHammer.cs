using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHammer : Weapon
{
    [SerializeField] private GameObject m_trail;
    [SerializeField] private int m_traiDamage = 10;

    DamageDealerInfo trailData;
    public override void StartHit()
    {
        canHit = true;
        ActiveSkill();
    }

    void ActiveSkill()
    {
        if(comboIndex < 0)
            return;
        SoundManager.PlaySound(m_clipFight[comboIndex], false);
        StartCoroutine(IBaseHit(comboIndex));
        if (comboIndex == (weaponCombos.Count - 1))
        {
            trailData = attackData;
            trailData.damage = m_traiDamage;
        }
    }
    
    IEnumerator IBaseHit(int indexCombo)
    {
        DamageDealerInfo data = attackData;
        List<Collider2D> hited = new List<Collider2D>();
        float startTime = Time.realtimeSinceStartup;
        bool sent = false;
        bool wallSent = false;
        bool monsterHited = false;

        while(canHit)
        {
            List<Collider2D> results = new List<Collider2D>();
            //enemy check
            weaponCombos[indexCombo].attackArea.OverlapCollider(enemyContactFilter, results);
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
                        data.attacker.SendMessage(EVENT_BEAT_WALL, (Vector3)results[i].ClosestPoint(wallAttackArea.bounds.center));
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

    public override void ExtraAttack()
    {
        //Spawn 4 thunder zap foward
        int direction = (int)Mathf.Sign(player.transform.localScale.x);// player.FaceDirection > 0 ? 1 : -1;
        GameObject trail = Instantiate(m_trail, player.transform.position + new Vector3(direction, 0), Quaternion.identity, player.parent);
        trail.SetActive(true);
        trail.GetComponent<BattleAxeTrail>().Init(enemyContactFilter, trailData, direction);
    }
}
