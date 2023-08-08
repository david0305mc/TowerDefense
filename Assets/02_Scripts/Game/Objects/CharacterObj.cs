using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using MonsterLove.StateMachine;

public class CharacterObj : BaseObj
{
    public enum FSMStates
    {
        Idle,
        Walk,
        Attack,
    }

    private CancellationTokenSource cts;

    private float speed = 5;
    public int attackRange = 3;
    private GroundManager.Path _path;
    private int _currentNodeIndex;
    private int wayPointIndex = 0;

    private int targetUID = -1;
    StateMachine<FSMStates, StateDriverUnity> fsm;

    private float commonDelay;
    void Awake()
    {
        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
    }

    void Update()
    {
        fsm.Driver.Update.Invoke();
    }

    void Idle_Enter()
    {
        commonDelay = 0f;
        UpdateRenderQuads();
    }
    void Idle_Update()
    {
        if (BaseObjData.IsEnemy)
            return;
        if (BaseObjData.RefObjData.object_type == OBJ_TYPE.BUILDING)
            return;

        commonDelay += Time.deltaTime;
        if (commonDelay >= 0f)
        {
            commonDelay = 0f;
            //CheckTarget();
            CheckWayPoint();
        }
    }
    private void CheckWayPoint()
    {
        var targetObj =  GameManager.Instance.GetNextWayPoint(wayPointIndex++);
        if (targetObj != null)
        {
            targetUID = targetObj.BaseObjData.UID;
            fsm.ChangeState(FSMStates.Walk);
        }
    }

    private void CheckTarget()
    {
        var targetObj = GameManager.Instance.GetnearestTarget(BaseObjData.UID);
        if (targetObj != null)
        {
            targetUID = targetObj.BaseObjData.UID;
            fsm.ChangeState(FSMStates.Walk);
        }
    }

    void Walk_Enter()
    {
        var targetObj = GameManager.Instance.GetBaseObj(targetUID);
        var targetPos = GroundManager.Instance.GetNearestOutCell(transform.position, targetObj.transform.position, 1);

        var path = GroundManager.Instance.GetPath(transform.position, targetPos, false);
        if (path.nodes == null || path.nodes.Length == 0)
        {
            // Check 
            FinishWalk();
            return;
        }

        //_baseItem.SetState(GameData.State.WALK);
        _path = path;
        _currentNodeIndex = 0;
        if (path != null || path.nodes != null && path.nodes.Length > 0)
        {
            //MoveToPosition(_path.nodes[_currentNodeIndex]);
            LookAt(_path.nodes[_currentNodeIndex]);
        }
        UpdateRenderQuads();
    }

    void Walk_Update()
    {
        float frameDistance = Time.deltaTime * speed;
        float interpolationValue = frameDistance / (_path.nodes[_currentNodeIndex] - transform.localPosition).magnitude;
        transform.localPosition = Vector3.Lerp(transform.localPosition, _path.nodes[_currentNodeIndex], interpolationValue);

        if (transform.localPosition == _path.nodes[_currentNodeIndex])
        {
            if (_path != null && _path.nodes != null && _currentNodeIndex < _path.nodes.Length - 1)
            {
                _currentNodeIndex++;
                //CheckTargetBeyondRange();
                //if (this.OnBetweenWalk != null)
                //{
                //    this.OnBetweenWalk.Invoke();
                //}
            }
            else
            {
                FinishWalk();
            }
        }
    }

    void Attack_Enter()
    {
        UpdateRenderQuads();
        UniTask.Create(async () =>
        {
            await UniTask.Delay(1000);

            if (UserData.Instance.LocalData.HasObj(targetUID))
            {
                GameManager.Instance.DetroyEnemy(targetUID);
            }
            targetUID = -1;
            fsm.ChangeState(FSMStates.Idle);
        });
    }

    public override void StartFSM()
    {
        fsm.ChangeState(FSMStates.Idle);
    }

    public void CheckTargetBeyondRange()
    {
        var targetObj = GameManager.Instance.GetBaseObj(targetUID);
        if (targetObj != null)
        {
            if (Vector3.Distance(targetObj.transform.position, transform.position) <= attackRange)
            {
                StartAttack();
            }
        }
    }

    public void CheckWallsOnRoot()
    {
        //		Debug.Log ("CheckWallsOnRoot");
        //if (Vector3.Distance(_currentTarget.GetCenterPosition(), this._baseItem.GetPosition()) <= this._baseItem.itemData.configuration.attackRange)
        //{
        //    this._baseItem.Walker.CancelWalk();
        //    this.AttackLoop();
        //}
    }

    private void FinishWalk()
    {
        cts?.Clear();
        cts = null;

        targetUID = -1;
        fsm.ChangeState(FSMStates.Idle);

        //StartAttack();
    }

    protected override DataManager.SpriteCollection GetSpriteCollection()
    {
        int collectionID;
        switch (fsm.State)
        {
            case FSMStates.Attack:
                collectionID = BaseObjData.RefObjData.attack_collectionid;
                break;
            case FSMStates.Walk:
                collectionID = BaseObjData.RefObjData.walk_collectionid;
                break;
            default:
                collectionID = BaseObjData.RefObjData.idle_collectionid;
                break;
        }
        return DataManager.Instance.GetSpriteCollectionData(collectionID);
    }

    private void StartAttack()
    {
        fsm.ChangeState(FSMStates.Attack);
    }

    private void OnDestroy()
    {
        cts?.Clear();
        cts = null;
    }
}