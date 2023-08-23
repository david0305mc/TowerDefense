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
    public enum FSMStates
    {
        Idle,
        WaypointMove,
        DashMove,
        Attack,
        AttackDelay,
    }

    private SwordAttackChecker swordAttackChecker;
    private StateMachine<FSMStates, StateDriverUnity> fsm;
    
    public GameObject targetWayPoint;
    public Vector2 targetoffset;
    private NavMeshPath currNavPath;
    private List<int> blackList;
    private int wayPointIndex;
    public string state;

    static readonly float agentDrift = 0.0001f; // minimal


    protected override void Awake()
    {
        blackList = new List<int>();
        currNavPath = new NavMeshPath();
        base.Awake();
        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
        swordAttackChecker = GetComponentInChildren<SwordAttackChecker>(true);
        if (swordAttackChecker != null)
        {
            swordAttackChecker.SetAttackAction(collision =>
            {
                if (fsm.State != FSMStates.Attack)
                {
                    return;
                }

                // Attack
                var damagable = collision.GetComponent<Damageable>();
                if (damagable != null)
                {
                    if (damagable.IsEnemy())
                    {
                        MGameManager.Instance.ShowBoomEffect(0, collision.ClosestPoint(transform.position));
                        damagable.GetDamaged(1);

                        var enemyData = UserData.Instance.GetEnemyData(targetObjUID);
                        if (enemyData == null)
                        {
                            fsm.ChangeState(FSMStates.Idle);
                        }
                    }
                }
            });
        }
        animationLink.SetEvent(() =>
        {
            if (fsm.State != FSMStates.Attack)
            {
                return;
            }
            // Fire Only For Projectile
            var enemyData = UserData.Instance.GetEnemyData(targetObjUID);
            if (enemyData != null)
            {
                if (unitData.refData.unit_type == UNIT_TYPE.ARCHER)
                {
                    MGameManager.Instance.LauchProjectileToEnemy(this, targetObjUID);
                }
            }

        }, ()=> {

            // Attack Ani End
            if (fsm.State != FSMStates.Attack)
            {
                return;
            }

            attackLongDelayCount--;
            if (attackLongDelayCount <= 0)
            {
                commonDelay = unitData.refUnitGradeData.attacklongdelay * 0.1f;
                attackLongDelayCount = unitData.refUnitGradeData.attackcount;
            }
            else
            {
                commonDelay = unitData.refUnitGradeData.attackshortdelay * 0.1f;
            }

            var enemyData = UserData.Instance.GetEnemyData(targetObjUID);
            if (enemyData != null)
            {
                fsm.ChangeState(FSMStates.AttackDelay);
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }
        });
    }

    public override bool IsEnemy()
    {
        return false;
    }

    public override void StartFSM()
    {
        wayPointIndex = 0;
        fsm.ChangeState(FSMStates.Idle);
    }
    void Update()
    {
        fsm.Driver.Update.Invoke();
    }

    void Idle_Enter()
    {
        PlayAni("Idle");
        agent.isStopped = true;
        attackLongDelayCount = unitData.refUnitGradeData.attackcount;
        commonDelay = 0f;
        state = fsm.State.ToString();
    }
    void Idle_Update()
    {
        commonDelay -= Time.deltaTime;
        if (commonDelay <= 0)
        {
            if (MGameManager.Instance.WayPoints.Count > wayPointIndex)
            {
                targetWayPoint = MGameManager.Instance.WayPoints[wayPointIndex].gameObject;
                agent.SetDestination(FixStuckPos(targetWayPoint.transform.position));
                fsm.ChangeState(FSMStates.WaypointMove);
            }
        }
    }

    void WaypointMove_Enter()
    {
        PlayAni("Walk");
        agent.isStopped = false;
        state = fsm.State.ToString();
    }

    void WaypointMove_Update()
    {
        agent.SetDestination(FixStuckPos(targetWayPoint.transform.position));
        FlipRenderers(agent.velocity.x < 0);
        
        var detectedObjs = Physics2D.OverlapCircleAll(transform.position, unitData.refData.checkrange, Game.GameConfig.UnitLayerMask);
        if (detectedObjs.Length > 0)
        {
            var objLists = detectedObjs.Where(item=>item.GetComponent<MEnemyObj>() != null).Select(item => item.GetComponent<MEnemyObj>());
            var enemyObj = GetNearestEnemyByAggro(objLists);

            if (enemyObj != null)
            {
                targetObjUID = enemyObj.UID;
                fsm.ChangeState(FSMStates.DashMove);
                return;
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

    void DashMove_Enter()
    {
        PlayAni("Walk");
        agent.isStopped = false;
        state = fsm.State.ToString();
    }
    
    void DashMove_Update()
    {
        MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
        if (enemyObj != null)
        {
            FlipRenderers(agent.velocity.x < 0);
            var speed = MGameManager.Instance.GetTileWalkingSpeed(transform.position);
            agent.speed = speed;
            if (!agent.SetDestination(FixStuckPos(enemyObj.transform.position)))
            {
                Debug.Log("Error");
            }

            var detectedObjs = Physics2D.OverlapCircleAll(transform.position, unitData.refData.checkrange, Game.GameConfig.UnitLayerMask);
            if (detectedObjs.Length > 0)
            {
                var objLists = detectedObjs.Where(item => item.GetComponent<MEnemyObj>() != null).Select(item => item.GetComponent<MEnemyObj>());
                MEnemyObj findEnemyObj = GetNearestEnemyByAggro(objLists);
                if (findEnemyObj != null)
                {
                    targetObjUID = findEnemyObj.UID;
                }
            }

            if (Vector2.Distance(transform.position, enemyObj.transform.position) < unitData.refUnitGradeData.attackrange + 0.01f)
            {
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
        agent.isStopped = true;
        PlayAni("Attack");
        
        LookTarget();
        state = fsm.State.ToString();
    }

    void Attack_Update()
    {

    }
    void AttackDelay_Enter()
    {
        PlayAni("Idle");
    }

    void AttackDelay_Update()
    {
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

    private MEnemyObj GetNearestEnemyByAggro(IEnumerable<MEnemyObj> enemyObjs)
    {
        if (enemyObjs.Count() == 0)
            return null;

        int maxAggroOrder = enemyObjs.Max(item => item.UnitData.refData.aggroorder);
        enemyObjs = enemyObjs.Where(item => item.UnitData.refData.aggroorder == maxAggroOrder);
        float nearestDist = float.MaxValue;
        MEnemyObj nearestEnemy = default;
        foreach (var item in enemyObjs)
        {
            float dist = Vector2.Distance(transform.position, item.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestEnemy = item;
            }
        }
        return nearestEnemy;
    }

    private Vector3 FixStuckPos(Vector3 _pos)
    {
        if (Mathf.Abs(transform.position.x - _pos.x) < agentDrift)
        {
            _pos = _pos + new Vector3(agentDrift, 0f, 0f);
        }
        return new Vector3(_pos.x, _pos.y, 0);
    }
}
