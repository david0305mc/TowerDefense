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

    public DataManager.Character refData { get; set; }

    StateMachine<FSMStates, StateDriverUnity> fsm;

    private float commonDelay;

    protected override void Awake()
    {
        base.Awake();
        
        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
        animationLink.SetFireEvent(() =>
        {
            var enemyData = UserData.Instance.GetHeroData(targetObjUID);
            if (enemyData != null)
            {
                if (refData.charactertype == CHARACTER_TYPE.ARCHER)
                {
                    MGameManager.Instance.LauchProjectileToHero(this, targetObjUID);
                }
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }

        });
    }
    public void InitObject(int _uid, System.Action _getDamageAction)
    {
        uid = _uid;
        getDamageAction = _getDamageAction;
        refData = DataManager.Instance.GetCharacterData(TID);
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
            DetectEnemy();
        }
    }
    private void DetectEnemy()
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
