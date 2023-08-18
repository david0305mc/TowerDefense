using Cysharp.Threading.Tasks;
using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class MHeroObj : MBaseObj
{
    public enum FSMStates
    {
        Idle,
        Move,
        AttackMove,
        Attack,
    }

    private SwordAttackChecker swordAttackChecker;
    private StateMachine<FSMStates, StateDriverUnity> fsm;


    private DataManager.Character refData;
    public Vector2 targetWorldPos;
    

    protected override void Awake()
    {
        isEnemy = false;
        base.Awake();
        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
        swordAttackChecker = GetComponentInChildren<SwordAttackChecker>(true);
        animationLink.SetFireEvent(() =>
        {
            var enemyData = UserData.Instance.GetEnemyData(targetObjUID);
            if (enemyData != null)
            {
                if (refData.charactertype == CHARACTER_TYPE.ARCHER)
                {
                    MGameManager.Instance.LauchProjectileToEnemy(this, targetObjUID);
                }
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }

        });
    }

    public void InitObject(int _uid, System.Action _getDamageAction)
    {
        uid = _uid;
        getDamageAction = _getDamageAction;
        refData = DataManager.Instance.GetCharacterData(TID);
    }

    public void StartFSM()
    {
        fsm.ChangeState(FSMStates.Idle);
    }
    void Update()
    {
        fsm.Driver.Update.Invoke();
    }

    void Idle_Enter()
    {
        animator.Play("char_01_idle");
        commonDelay = 0f;

    }
    void Idle_Update()
    {
        commonDelay += Time.deltaTime;
        if (commonDelay >= 1f)
        {
            DetectEnemy();
        }
    }

    private void DetectEnemy()
    {
        commonDelay = 0;
        targetObjUID = MGameManager.Instance.GetNearestEnemyObj(transform.position);
        MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
        if (enemyObj != null)
        {
            float randX = Random.Range(0.5f, 1.5f);
            float randY = Random.Range(-1, 2);

            Vector3 pos01 = enemyObj.transform.position + new Vector3(randX, randY, 0);
            Vector3 pos02 = enemyObj.transform.position + new Vector3(-randX, randY, 0);
            if (Vector3.Distance(transform.position, pos01) < Vector3.Distance(transform.position, pos02))
            {
                targetWorldPos = pos01;
                targetWorldPos = enemyObj.transform.position;
                fsm.ChangeState(FSMStates.Move);
            }
            else
            {
                targetWorldPos = pos02;
                targetWorldPos = enemyObj.transform.position;
                fsm.ChangeState(FSMStates.Move);
            }
        }
    }

    void Move_Enter()
    {
        animator.Play("char_01_walk");
        agent.isStopped = false;
        SetDestination(targetWorldPos);
    }
    static float agentDrift = 0.0001f; // minimal
    void SetDestination(Vector3 target)
    {
        Vector3 driftPos = target;
        if (Mathf.Abs(transform.position.x - target.x) < agentDrift)
        {
            driftPos = target + new Vector3(agentDrift, 0f, 0f);
        }
        agent.SetDestination(driftPos);
    }
    void Move_Update()
    {
        MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
        var speed = MGameManager.Instance.GetTileWalkingSpeed(transform.position);
        agent.speed = speed;

        FlipRenderers(agent.velocity.x < 0);
        if (enemyObj != null)
        {
            //FlipRenderers(transform.position.x > enemyObj.transform.position.x);
            if (Vector2.Distance(transform.position, targetWorldPos) < refData.attackrange * 0.1f + 0.01f)
            {
                agent.isStopped = true;
                fsm.ChangeState(FSMStates.Attack);
            }
        }
        else
        {
            fsm.ChangeState(FSMStates.Idle);
        }
        
    }


    void Attack_Enter()
    {
        animator.Play("char_01_atk");
        commonDelay = 0;
    }

    void Attack_Update()
    {
        
    }
}
