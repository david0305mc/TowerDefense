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
    StateMachine<FSMStates, StateDriverUnity> fsm;

    [SerializeField] private Animator animator;
    [SerializeField] private AnimationLink animationLink;
    [SerializeField] private NavMeshAgent agent;

    private float commonDelay;
    private int testCnt;
    Vector2 targetWorldPos;
    int targetObjUID;
    

    void Awake()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
        animationLink.SetFireEvent(() =>
        {
            var enemyData = UserData.Instance.GetEnemyData(targetObjUID);
            if (enemyData != null)
            {
                MGameManager.Instance.LauchProjectile(this, targetObjUID);
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }

        });
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
            float randX = Random.Range(3, 6);
            float randY = Random.Range(-3, 4);

            targetWorldPos = enemyObj.transform.position + new Vector3(randX, randY, 0);
            fsm.ChangeState(FSMStates.Move);
        }
    }

    void Move_Enter()
    {
        animator.Play("char_01_walk");
        agent.isStopped = false;
        agent.SetDestination(targetWorldPos);
    }
    void Move_Update()
    {
        MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
        if (enemyObj != null)
        {
            FlipRenderers(transform.position.x > enemyObj.transform.position.x);
            if (Vector2.Distance(transform.position, targetWorldPos) < enemyObj.refData.attackrange)
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
        testCnt = 0;
    }

    void Attack_Update()
    {
        
    }
}
