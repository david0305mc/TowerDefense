using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class MEnemyObj : MBaseObj
{

    [SerializeField] private float rangeCheckForEditor = 3f;
    

    public override bool IsEnemy()
    {
        return UnitData.IsEnemy;
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
        agent.velocity = Vector3.zero;
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
    protected override void DoSwordAttack(Collider2D collision)
    {
        var damagable = collision.GetComponent<Damageable>();
        if (damagable != null)
        {
            if (!damagable.IsEnemy())
            {
                var attackData = new AttackData(this.unitData.uid, this.unitData.tid, this.unitData.refUnitGradeData.attackdmg, !UnitData.IsEnemy);
                MGameManager.Instance.ShowBoomEffect(0, attackData, collision.ClosestPoint(transform.position));
                damagable.GetDamaged(attackData);

                var unitData = UserData.Instance.GetHeroData(targetObjUID);
                if (unitData == null)
                {
                    fsm.ChangeState(FSMStates.Idle);
                }
            }
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

            if (Vector2.Distance(transform.position, heroObj.transform.position) < unitData.refUnitGradeData.attackrange * 0.1f + 0.01f)
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
        agent.velocity = Vector3.zero;
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

    public void DoDashMove(int _targetUID)
    {
        targetObjUID = _targetUID;
        fsm.ChangeState(FSMStates.DashMove);
    }

    public override void DoAggro(int _attackerUID)
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
