using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class CharacterObj : BaseObj
{
    private CancellationTokenSource cts;

    private float speed = 5;
    private GroundManager.Path _path;
    private int _currentNodeIndex;

    private int targetUID = -1;

    public override void StartStateMachine()
    {

        if (BaseObjData.IsEnemy)
            return;

        cts?.Clear();
        cts = new CancellationTokenSource();

        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.Update).ForEachAsync(_ =>
        {
            var targetObj = GameManager.Instance.GetnearestTarget(BaseObjData.UID);
            if (targetObj != null)
            {
                var targetPos = GroundManager.Instance.GetNearestOutCell(targetObj.transform.position, 1);
                targetUID = targetObj.BaseObjData.UID;
                WalkThePath(GroundManager.Instance.GetPath(transform.position, targetPos, false));
            }
        }, cts.Token);
    }

    private void MoveToPosition(Vector3 targetPosition)
    {
        cts?.Clear();
        cts = new CancellationTokenSource();
        BaseObjData.ObjStatus = Game.ObjStatus.Walk;
        LookAt(targetPosition);
        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.Update).ForEachAsync(_ =>
        {
            float frameDistance = Time.deltaTime * speed;
            float interpolationValue = frameDistance / (targetPosition - transform.localPosition).magnitude;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, interpolationValue);
            
            if (transform.localPosition == targetPosition)
            {
                WalkNextNode();
            }
        }, cts.Token);
    }

    private void WalkNextNode()
    {
        if (_path != null && _path.nodes != null && _currentNodeIndex < _path.nodes.Length - 1)
        {
            _currentNodeIndex++;
            MoveToPosition(_path.nodes[_currentNodeIndex]);
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


    public void WalkToPosition(Vector3 position)
    {
        //CancelWalk();
        WalkThePath(GroundManager.Instance.GetPath(transform.localPosition, position, false));
    }

    private void WalkThePath(GroundManager.Path path)
    {
        if (path.nodes == null || path.nodes.Length == 0)
        {
            FinishWalk();
            return;
        }

        //_baseItem.SetState(GameData.State.WALK);
        _path = path;
        _currentNodeIndex = 0;
        if (path != null || path.nodes != null && path.nodes.Length > 0)
        {
            MoveToPosition(_path.nodes[0]);
        }
    }

    private void FinishWalk()
    {
        cts?.Clear();
        cts = null;

        BaseObjData.ObjStatus = Game.ObjStatus.Attack;
        UpdateRenderQuads();

        UniTask.Create(async () => {
            await UniTask.Delay(1000);

            if (UserData.Instance.LocalData.HasObj(targetUID))
            {
                GameManager.Instance.DetroyEnemy(targetUID);
            }
            targetUID = -1;
            StartStateMachine();
        });

        //_baseItem.SetState(GameData.State.IDLE);
        //_isWalking = false;
        //_targetPosition = transform.position;
        //if (OnFinishWalk != null)
        //{
        //    OnFinishWalk.Invoke();
        //}

        //OnBetweenWalk = null;
    }

    //public void Attack(BaseObj _target)
    //{
    //    var targetCell = GroundManager.Instance.GetNearestOutCell(_target.transform.localPosition, 1);
    //    WalkToPosition(targetCell);
    //}

    private void OnDestroy()
    {
        cts?.Clear();
        cts = null;
    }
}