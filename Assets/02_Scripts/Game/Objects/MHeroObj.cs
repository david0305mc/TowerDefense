using Cysharp.Threading.Tasks;
using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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

    private float commonDelay;
    private int testCnt;
    Vector2 targetWorldPos;
    MEnemyObj targetObj;
    

    void Awake()
    {
        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
        animationLink.SetFireEvent(() =>
        {
            var enemyData = UserData.Instance.GetEnemyData(targetObj.UID);
            if (enemyData != null)
            {
                MGameManager.Instance.LauchProjectile(this, targetObj);
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
        targetObj = MGameManager.Instance.GetNearestEnemyObj(transform.position);

        if (targetObj != null)
        {
            float randX = Random.Range(3, 6);
            float randY = Random.Range(-3, 4);

            targetWorldPos = targetObj.transform.position + new Vector3(randX, randY, 0);
            fsm.ChangeState(FSMStates.Move);
        }
    }

    void Move_Enter()
    {
        animator.Play("char_01_walk");
    }
    void Move_Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * 3f);
        FlipRenderers(transform.position.x > targetObj.transform.position.x);
        if (Vector2.Distance(transform.position, targetWorldPos) < 0.1f)
        {
            transform.position = targetWorldPos;
            fsm.ChangeState(FSMStates.Attack);
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
