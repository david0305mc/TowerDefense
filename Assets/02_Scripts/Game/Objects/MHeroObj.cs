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
    public Vector2 targetoffset;
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
                MGameManager.Instance.ShowBoomEffect(0, attackData, collision.ClosestPoint(transform.position));
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
        PlayAni("Idle");
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        attackLongDelayCount = unitData.refUnitGradeData.attackcount;
        commonDelay = 0f;
        state = fsm.State.ToString();
        agent.avoidancePriority = 11;
        isFixedTarget = false;
    }
    protected override void Idle_Update()
    {
        commonDelay -= Time.deltaTime;
        if (commonDelay <= 0)
        {
            if (MGameManager.Instance.WayPoints.Count > wayPointIndex)
            {
                targetWayPoint = MGameManager.Instance.WayPoints[wayPointIndex].gameObject;
                agent.SetDestination(GetFixedStuckPos(targetWayPoint.transform.position));
                fsm.ChangeState(FSMStates.WaypointMove);
            }
        }
    }

    void WaypointMove_Enter()
    {
        PlayAni("Walk");
        agent.isStopped = false;
        state = fsm.State.ToString();
        agent.avoidancePriority = 11;
    }

    void WaypointMove_Update()
    {
        UpdateAgentSpeed();
        agent.SetDestination(GetFixedStuckPos(targetWayPoint.transform.position));
        FlipRenderers(agent.velocity.x < 0);

        if (!isFixedTarget)
        {
            var detectedObjs = Physics2D.OverlapCircleAll(transform.position, unitData.refData.checkrange, Game.GameConfig.UnitLayerMask);
            if (detectedObjs.Length > 0)
            {
                var objLists = detectedObjs.Where(item => item.GetComponent<MEnemyObj>() != null).Select(item => item.GetComponent<MEnemyObj>());
                var enemyObj = GetNearestTargetByAggro(objLists);

                if (enemyObj != null)
                {
                    targetObjUID = enemyObj.UID;
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
        PlayAni("Walk");
        agent.isStopped = false;
        state = fsm.State.ToString();
        agent.avoidancePriority = 11;
    }

    protected override void DashMove_Update()
    {
        MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
        if (enemyObj != null)
        {
            FlipRenderers(agent.velocity.x < 0);
            UpdateAgentSpeed();
            agent.SetDestination(GetFixedStuckPos(enemyObj.transform.position));

            if (!isFixedTarget)
            {
                var detectedObjs = Physics2D.OverlapCircleAll(transform.position, unitData.refData.checkrange, Game.GameConfig.UnitLayerMask);
                if (detectedObjs.Length > 0)
                {
                    var objLists = detectedObjs.Where(item =>
                    {
                        MBaseObj baseObj = item.GetComponent<MBaseObj>();
                        return baseObj != null && baseObj.IsEnemy();
                    }).Select(item => item.GetComponent<MBaseObj>());

                    MBaseObj findTargetObj = GetNearestTargetByAggro(objLists);
                    if (findTargetObj != null)
                    {
                        targetObjUID = findTargetObj.UID;
                    }
                }
            }

            if (Vector2.Distance(transform.position, enemyObj.transform.position) < unitData.refUnitGradeData.attackrange * 0.1f + 0.01f)
            {
                fsm.ChangeState(FSMStates.Attack);
            }
        }
        else
        {
            fsm.ChangeState(FSMStates.Idle);
        }
    }


    protected override void Attack_Enter()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        PlayAni("Attack");
        LookTarget();
        state = fsm.State.ToString();
        agent.avoidancePriority = 11;
        commonDelay = 0f;
        isFixedTarget = true;
    }

    protected override void Attack_Update()
    {
        LookTarget();
    }
    protected override void AttackDelay_Enter()
    {
        PlayAni("Idle");
        agent.avoidancePriority = 11;
    }

    protected override void AttackDelay_Update()
    {
        LookTarget();
        commonDelay -= Time.deltaTime;
        if (commonDelay <= 0f)
        {
            MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
            if (enemyObj != null)
            {
                fsm.ChangeState(FSMStates.Attack);
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }
        }
    }
    public override void DoAggro(int _attackerUID)
    {
        if (!isFixedTarget)
        {
            targetObjUID = _attackerUID;
        }
        isFixedTarget = true;
        if (fsm.State == FSMStates.Idle || fsm.State == FSMStates.WaypointMove)
        {
            fsm.ChangeState(FSMStates.DashMove);
        }
    }

    private void LookTarget()
    {
        MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
        if (enemyObj != null)
        {
            FlipRenderers(enemyObj.transform.position.x < transform.position.x);
        }
    }

    private void ResetTrigger()
    {
        foreach (var p in animator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(p.name);
            }
        }
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
