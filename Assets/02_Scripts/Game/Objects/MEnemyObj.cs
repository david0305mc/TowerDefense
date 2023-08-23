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
        Move,
        AttackMove,
        Attack,
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
                var damagable = collision.GetComponent<Damageable>();
                if (damagable != null)
                {
                    if (!damagable.IsEnemy())
                    {
                        damagable.GetDamaged(1);
                        MGameManager.Instance.ShowBoomEffect(0, collision.ClosestPoint(swordAttackChecker.transform.position));
                    }
                }
            });
        }
        animationLink.SetEvent(() =>
        {
            var enemyData = UserData.Instance.GetHeroData(targetObjUID);
            if (enemyData != null)
            {
                if (unitData.refData.unit_type == UNIT_TYPE.ARCHER)
                {
                    MGameManager.Instance.LauchProjectileToHero(this, targetObjUID);
                }
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }
        }, ()=> {
            var enemyData = UserData.Instance.GetHeroData(targetObjUID);
            if (enemyData != null)
            {
                fsm.ChangeState(FSMStates.Idle);
            }
        });
    }
    public void InitObject(int _uid, System.Action _getDamageAction)
    {
        uid = _uid;
        getDamageAction = _getDamageAction;
        unitData = UserData.Instance.GetEnemyData(_uid);
        agent.isStopped = true;
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
        animator.Play("char_01_idle");
        commonDelay = 0f;
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

        var detectedObjs = Physics2D.OverlapCircleAll(transform.position, 5);
        if (detectedObjs.Length > 0)
        {
            var heroObj = detectedObjs.FirstOrDefault(item => { return item.GetComponent<MHeroObj>() != null; });

            if (heroObj != default)
            {
                targetObjUID = heroObj.GetComponent<MHeroObj>().UID;
                fsm.ChangeState(FSMStates.Attack);
                FlipRenderers(heroObj.transform.position.x < transform.position.x);
            }
        }        
    }
    void Attack_Enter()
    {
        animator.Play("char_01_atk");
        commonDelay = 0;
    }
    void Attack_Update()
    {

    }
}
