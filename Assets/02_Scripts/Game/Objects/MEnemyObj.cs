using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class MEnemyObj : MBaseObj
{
    public enum FSMStates
    {
        Idle,
        DashMove,
        Attack,
        AttackDelay,
    }

    [SerializeField] private float rangeCheckForEditor = 3f;
    
    StateMachine<FSMStates, StateDriverUnity> fsm;
    private SwordAttackChecker swordAttackChecker;

    public override bool IsEnemy()
    {
        return true;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0, 0, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(Vector3.zero, rangeCheckForEditor);

    }

    protected override void Awake()
    {
        base.Awake();
        
        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
        swordAttackChecker = GetComponentInChildren<SwordAttackChecker>(true);
        if (swordAttackChecker != null)
        {
            swordAttackChecker.SetAttackAction(collision =>
            {
                // Attack
                if (fsm.State != FSMStates.Attack)
                {
                    return;
                }

                var damagable = collision.GetComponent<Damageable>();
                if (damagable != null)
                {
                    if (!damagable.IsEnemy())
                    {
                        damagable.GetDamaged(new AttackData(unitData.uid, unitData.tid, unitData.refUnitGradeData.attackdmg));
                        MGameManager.Instance.ShowBoomEffect(0, collision.ClosestPoint(swordAttackChecker.transform.position));
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

            var targetUnitData = UserData.Instance.GetHeroData(targetObjUID);
            if (targetUnitData != null)
            {
                if (unitData.refData.unit_type == UNIT_TYPE.ARCHER)
                {
                    MGameManager.Instance.LauchProjectileToHero(this, targetObjUID);
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

            var targetUnitData = UserData.Instance.GetHeroData(targetObjUID);
            if (targetUnitData != null)
            {
                MHeroObj heroObj = MGameManager.Instance.GetHeroObj(targetObjUID);
                if (Vector2.Distance(transform.position, heroObj.transform.position) > unitData.refUnitGradeData.attackrange)
                {
                    fsm.ChangeState(FSMStates.DashMove);
                }
                else
                {
                    fsm.ChangeState(FSMStates.AttackDelay);
                }
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }
        });
    }

    public override void StartFSM()
    {
        fsm.ChangeState(FSMStates.Idle);
    }
    void Update()
    {
        fsm.Driver.Update.Invoke();
    }

    void Idle_Enter()
    {
        PlayAni("Idle");
        commonDelay = 0f;
        agent.isStopped = true;
        state = fsm.State.ToString();
        agent.avoidancePriority = 1;
        isFixedTarget = false;
    }
    void Idle_Update()
    {
        commonDelay += Time.deltaTime;
        if (commonDelay >= 1f)
        {
            DetectHero();
        }
    }
    private void DetectHero()
    {
        commonDelay = 0;
        
        var detectedObjs = Physics2D.OverlapCircleAll(transform.position, unitData.refData.checkrange);
        if (detectedObjs.Length > 0)
        {
            var heroObj = detectedObjs.FirstOrDefault(item => { return item.GetComponent<MHeroObj>() != null; });

            if (heroObj != default)
            {
                targetObjUID = heroObj.GetComponent<MHeroObj>().UID;
                fsm.ChangeState(FSMStates.DashMove);
                FlipRenderers(heroObj.transform.position.x < transform.position.x);
            }
        }        
    }
    void DashMove_Enter()
    {
        PlayAni("Walk");
        commonDelay = 0;
        agent.isStopped = false;
        state = fsm.State.ToString();
        agent.avoidancePriority = 1;
    }

    void DashMove_Update()
    {
        MHeroObj heroObj = MGameManager.Instance.GetHeroObj(targetObjUID);
        if (heroObj != null)
        {
            FlipRenderers(agent.velocity.x < 0);
            UpdateAgentSpeed();
            agent.SetDestination(GetFixedStuckPos(heroObj.transform.position));

            if (!isFixedTarget)
            {
                var detectedObjs = Physics2D.OverlapCircleAll(transform.position, unitData.refData.checkrange, Game.GameConfig.UnitLayerMask);
                if (detectedObjs.Length > 0)
                {
                    var objLists = detectedObjs.Where(item =>
                    {
                        MBaseObj baseObj = item.GetComponent<MBaseObj>();
                        return baseObj != null && !baseObj.IsEnemy();
                    }).Select(item => item.GetComponent<MBaseObj>());

                    MBaseObj findTargetObj = GetNearestTargetByAggro(objLists);
                    if (findTargetObj != null)
                    {
                        targetObjUID = findTargetObj.UID;
                    }
                }
            }

            if (Vector2.Distance(transform.position, heroObj.transform.position) < unitData.refUnitGradeData.attackrange + 0.01f)
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
        PlayAni("Attack");
        commonDelay = 0;
        agent.isStopped = true;
        LookTarget();
        state = fsm.State.ToString();
        agent.avoidancePriority = 1;
        isFixedTarget = true;
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
        LookTarget();
        commonDelay -= Time.deltaTime;
        if (commonDelay <= 0f)
        {
            MHeroObj targetObj = MGameManager.Instance.GetHeroObj(targetObjUID);
            if (targetObj != null)
            {
                fsm.ChangeState(FSMStates.Attack);
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }
        }
    }
    public override void DoDamage(int _attackerUID)
    {
        if (!isFixedTarget)
        {
            targetObjUID = _attackerUID;
        }
        isFixedTarget = true;
        if (fsm.State == FSMStates.Idle)
        {
            fsm.ChangeState(FSMStates.DashMove);
        }
    }
    private void LookTarget()
    {
        MHeroObj targetObj = MGameManager.Instance.GetHeroObj(targetObjUID);
        if (targetObj != null)
        {
            FlipRenderers(targetObj.transform.position.x < transform.position.x);
        }
    }
}
