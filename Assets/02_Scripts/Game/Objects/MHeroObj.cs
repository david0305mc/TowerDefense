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

    private float commonDelay;
    private int testCnt;
    Vector2 targetWorldPos;
    MEnemyObj targetObj;

    void Awake()
    {
        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
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
        targetObj = MapMangerTest.Instance.GetNearestEnemyObj(transform.position);

        if (targetObj != null)
        {
            float randX = Random.Range(1, 2);
            float randY = Random.Range(-2, 2);

            targetWorldPos = targetObj.transform.position + new Vector3(randX, randY, 0);
            fsm.ChangeState(FSMStates.Move);
        }
    }

    void Move_Enter()
    {

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
        commonDelay = 0;
        testCnt = 0;
    }

    void Attack_Update()
    {
        commonDelay += Time.deltaTime;
        if (commonDelay >= 1f)
        {
            testCnt++;
            commonDelay = 0;
            if (testCnt >= 5)
            {
                MapMangerTest.Instance.RemoveEnemy(targetObj);
                fsm.ChangeState(FSMStates.Idle);
            }
            else
            {
                MapMangerTest.Instance.LauchProjectile(this, targetObj);
            }
        }

        
    }
}
