using Cysharp.Threading.Tasks;
using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MHeroObj : MonoBehaviour
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
    Vector2 targetWorldPos;
    EnemyObj targetObj;

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
            float randX = Random.RandomRange(1, 2);
            float randY = Random.RandomRange(-2, 2);

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
        if (Vector2.Distance(transform.position, targetWorldPos) < 0.1f)
        {
            transform.position = targetWorldPos;
            MapMangerTest.Instance.RemoveEnemy(targetObj);
            fsm.ChangeState(FSMStates.Idle);
        }
    }

    private CancellationTokenSource cts;
    public void MoveTo(Vector3 targetPos)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        UniTask.Create(async () => {

            while (true)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
                if (Vector2.Distance(transform.position, targetPos) < 0.1f)
                {
                    cts.Cancel();
                    break;
                }
                await UniTask.WaitForFixedUpdate(cancellationToken: cts.Token);
            }
        });
    }
}
