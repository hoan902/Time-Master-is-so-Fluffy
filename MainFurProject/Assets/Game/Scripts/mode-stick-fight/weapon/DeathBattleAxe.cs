using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBattleAxe : Weapon
{
    [SerializeField] private GameObject m_trail;
    [SerializeField] private int m_traiDamage = 10;

    private void Start()
    {
        strainFullEvent += OnStrainFull;
    }
    private void OnDestroy()
    {
        strainFullEvent -= OnStrainFull;
    }

    public override void StartHit()
    {
        //base.StartHit();
        canHit = true;
        ActiveSkill();
    }

    void OnStrainFull()
    {
        GameController.ActivateInputTutorial(InputTutorialType.Hold, InputTutorialTarget.Fight, false);
    }

    void ActiveSkill()
    {
        switch (strainIndex)
        {
            case 0:
                SoundManager.PlaySound(m_clipFight[0], false);
                GameController.ShakeCameraWeak();
                StartCoroutine(IBaseHit(attackData));
                break;
            case 1:
                SoundManager.PlaySound(m_clipFight[1], false);
                GameController.VibrateCustom(new Vector3(0.2f, 0.2f), 0.2f);
                DamageDealerInfo trailData = attackData;
                trailData.damage = m_traiDamage;
                StartCoroutine(IBaseHit(attackData));
                if (player.isGround)
                    SpawnTrail(trailData);
                break;
        }
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
                        var parent = transform.parent;
                        parent.SendMessage(EVENT_BEAT_WALL, (Vector3)results[i].ClosestPoint(wallAttackArea.bounds.center));
                        parent.SendMessage(EVENT_WALL_KNOCKBACK);
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
                    transform.parent.SendMessage(EVENT_BEAT_CRITICAL);
                    GameController.ShakeCamera();
                }
                else
                {
                    transform.parent.SendMessage(EVENT_BEAT_NORMAL);
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

    void SpawnTrail(DamageDealerInfo data)
    {
        int direction = (int)Mathf.Sign(player.transform.localScale.x);// player.FaceDirection > 0 ? 1 : -1;
        GameObject trail = Instantiate(m_trail, player.transform.position + new Vector3( direction, 0), Quaternion.identity, player.parent);
        trail.SetActive(true);
        trail.GetComponent<BattleAxeTrail>().Init(enemyContactFilter, data, direction);
    }
}
