using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class MEnemyObj : MBaseObj
{

    [SerializeField] private float rangeCheckForEditor = 3f;
    

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
        base.StartFSM();
    }
    protected override void Idle_Enter()
    {
        base.Idle_Enter();
        agent.avoidancePriority = 1;
    }
    protected override void Idle_Update()
    {
        commonDelay += Time.deltaTime;
        if (commonDelay >= 0.1f)
        {
            DetectHero();
        }
    }
    protected override void WaypointMove_Enter()
    {
        Debug.LogError("Enemy Doesnt Have WaypointState");
    }
    protected override void WaypointMove_Update()
    {
        Debug.LogError("Enemy Doesnt Have WaypointState Update");
    }
    protected override void DoSwordAttack(Collider2D collision)
    {
        var damagable = collision.GetComponent<Damageable>();
        if (damagable != null)
        {
            if (!damagable.IsEnemy())
            {
                var attackData = new AttackData(this.unitData.uid, this.unitData.tid, this.unitData.refUnitGradeData.attackdmg, !UnitData.IsEnemy);
                MGameManager.Instance.ShowBoomEffect(attackData, collision.ClosestPoint(transform.position));
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
        var detectedObjs = FindUnitListByArea(unitData.refData.checkrange, false);
        if (detectedObjs != default && detectedObjs.Count > 0)
        {
            var heroObj = FindNearestTargetByAggroOrder(detectedObjs);
            if (heroObj != default)
            {
                if(!SetTargetObject(heroObj.UID))
                {
                    return;
                }
                fsm.ChangeState(FSMStates.DashMove);
                FlipRenderers(heroObj.transform.position.x < transform.position.x);
            }
        }
    }
    protected override void DashMove_Enter()
    {
        base.DashMove_Enter();
        agent.avoidancePriority = 1;
    }

    protected override void DashMove_Update()
    {
        base.DashMove_Update();
    }

    protected override void Attack_Enter()
    {
        base.Attack_Enter();
        agent.avoidancePriority = 1;
    }
    protected override void Attack_Update()
    {
        base.Attack_Update();
    }

    protected override void AttackDelay_Enter()
    {
        base.AttackDelay_Enter();
        agent.avoidancePriority = 1;
    }

    protected override void AttackDelay_Update()
    {
        base.AttackDelay_Update();
    }
}
