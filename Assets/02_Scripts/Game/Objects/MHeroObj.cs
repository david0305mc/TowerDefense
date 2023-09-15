using Cysharp.Threading.Tasks;
using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus;
using System.Linq;

public class MHeroObj : MBaseObj
{
    public GameObject targetWayPoint;
    private int wayPointIndex;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void StartFSM()
    {
        wayPointIndex = 0;
        base.StartFSM();
    }

    protected override void DoSwordAttack(Collider2D collision)
    {
        // Attack
        var damagable = collision.GetComponent<Damageable>();
        if (damagable != null)
        {
            if (damagable.IsEnemy())
            {
                var attackData = new AttackData(this.unitData.uid, this.unitData.tid, this.unitData.refUnitGradeData.attackdmg, !UnitData.IsEnemy);
                MGameManager.Instance.ShowBoomEffect(attackData, collision.ClosestPoint(transform.position));
                damagable.GetDamaged(attackData);

                var unitData = UserData.Instance.GetEnemyData(targetObjUID);
                if (unitData == null)
                {
                    fsm.ChangeState(FSMStates.Idle);
                }
            }
        }
    }

    protected override void Idle_Enter()
    {
        base.Idle_Enter();
        agent.avoidancePriority = 11;
    }
    protected override void Idle_Update()
    {
        base.Idle_Update();
        commonDelay -= Time.deltaTime;
        if (commonDelay <= 0)
        {
            if (MGameManager.Instance.WayPoints.Count > wayPointIndex)
            {
                targetWayPoint = MGameManager.Instance.WayPoints[wayPointIndex].gameObject;
                fsm.ChangeState(FSMStates.WaypointMove);
            }
        }
    }

    protected override void WaypointMove_Enter()
    {
        PlayAni("Walk");
        agent.isStopped = false;
        state = fsm.State.ToString();
        agent.avoidancePriority = 11;
    }

    protected override void WaypointMove_Update()
    {
        UpdateAgentSpeed();
        DoAgentMove(targetWayPoint.transform.position);
        FlipRenderers(agent.velocity.x < 0);
 
        var targetLists = FindUnitListByArea(unitData.refData.checkrange, true);
        if (targetLists.Count > 0)
        {
            var enemyObj = FindNearestTargetByAggroOrder(targetLists);
            if (enemyObj != null)
            {
                if (SetTargetObject(enemyObj.UID))
                {
                    fsm.ChangeState(FSMStates.DashMove);
                    return;
                }
            }
        }
        
        if (Vector2.Distance(transform.position, targetWayPoint.transform.position) < 0.3f)
        {
            wayPointIndex++;

            if (MGameManager.Instance.WayPoints.Count > wayPointIndex)
            {
                targetWayPoint = MGameManager.Instance.WayPoints[wayPointIndex].gameObject;
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }
        }
    }

    protected override void DashMove_Enter()
    {
        base.DashMove_Enter();
        agent.avoidancePriority = 11;
    }

    protected override void DashMove_Update()
    {
        base.DashMove_Update();
    }

    protected override void Attack_Enter()
    {
        base.Attack_Enter();
        agent.avoidancePriority = 11;
    }

    protected override void Attack_Update()
    {
        base.Attack_Update();
    }
    protected override void AttackDelay_Enter()
    {
        base.AttackDelay_Enter();
        agent.avoidancePriority = 11;
    }

    protected override void AttackDelay_Update()
    {
        base.AttackDelay_Update();
    }

    protected override bool SetTargetObject(int _uid)
    {
        return base.SetTargetObject(_uid);
    }



    //private void DetectEnemy()
    //{
    //    commonDelay = 0;
    //    targetObjUID = MGameManager.Instance.GetNearestEnemyObj(transform.position, blackList);

    //    MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
    //    if (enemyObj != null)
    //    {
    //        if (!agent.CalculatePath(enemyObj.transform.position, currNavPath))
    //        {
    //            blackList.Add(targetObjUID);
    //        }
    //        else
    //        {
    //            float randX = Random.Range(0.5f, 1.5f);
    //            float randY = Random.Range(-1, 2);

    //            Vector3 pos01 = enemyObj.transform.position + new Vector3(randX, randY, 0);
    //            Vector3 pos02 = enemyObj.transform.position + new Vector3(-randX, randY, 0);
    //            if (Vector3.Distance(transform.position, pos01) < Vector3.Distance(transform.position, pos02))
    //            {
    //                //targetWorldPos = FixStuckPos(pos01);
    //                //targetWayPoint = enemyObj.gameObject;
    //            }
    //            else
    //            {
    //                //targetWorldPos = FixStuckPos(pos02);
    //                //targetWayPoint = enemyObj.gameObject;
    //            }

    //            fsm.ChangeState(FSMStates.DashMove);
    //        }
    //    }
    //}

}
