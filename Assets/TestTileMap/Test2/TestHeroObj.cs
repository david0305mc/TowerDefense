using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using MonsterLove.StateMachine;


public class TestHeroObj : MonoBehaviour
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
    private TestGroundManager.Path path;
    private int pathIndex;

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

    }

    public void MoveTo(TestGroundManager.Path _path)
    {
        pathIndex = 0;
        path = _path;
        fsm.ChangeState(FSMStates.Move);
    }
    void Move_Enter()
    {
    }
    void Move_Update()
    {
        var targetWorldPos = MapMangerTest.Instance.tileMap.GetCellCenterWorld(new Vector3Int((int)path.nodes[pathIndex].x, (int)path.nodes[pathIndex].z, (int)path.nodes[pathIndex].y));
        targetWorldPos = new Vector3(targetWorldPos.x, targetWorldPos.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * 30f);
        if (Vector2.Distance(transform.position, targetWorldPos) < 0.1f)
        {
            transform.position = targetWorldPos;
            pathIndex++;
            if (pathIndex >= path.nodes.Length)
            {
                fsm.ChangeState(FSMStates.Idle);
            }
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
