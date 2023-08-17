using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private System.Action getDamageAction;
    StateMachine<FSMStates, StateDriverUnity> fsm;

    private float commonDelay;

    protected override void Awake()
    {
        base.Awake();
        
        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
        animationLink.SetFireEvent(() =>
        {
            //var enemyData = UserData.Instance.GetEnemyData(targetObjUID);
            //if (enemyData != null)
            //{
            //    if (refData.charactertype == CHARACTER_TYPE.ARCHER)
            //    {
            //        MGameManager.Instance.LauchProjectile(this, targetObjUID);
            //    }
            //}
            //else
            //{
            //    fsm.ChangeState(FSMStates.Idle);
            //}

        });
    }
    public void InitObject(int _uid, System.Action _getDamageAction)
    {
        uid = _uid;
        getDamageAction = _getDamageAction;
        refData = DataManager.Instance.GetCharacterData(TID);
        agent.isStopped = true;
    }

    public override void GetDamaged(int _damage)
    {
        getDamageAction?.Invoke();
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

        var detectedObjs = Physics2D.OverlapCircleAll(transform.position, 3);
        if (detectedObjs.Length > 0)
        {   
            var heroObj = detectedObjs[0].GetComponent<MHeroObj>();
            targetObjUID = heroObj.UID;
            fsm.ChangeState(FSMStates.Attack);
        }
        //targetObjUID = MGameManager.Instance.GetNearestEnemyObj(transform.position);
        //MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
        //if (enemyObj != null)
        //{
        //    float randX = Random.Range(0.5f, 1.5f);
        //    float randY = Random.Range(-1, 2);

        //    Vector3 pos01 = enemyObj.transform.position + new Vector3(randX, randY, 0);
        //    Vector3 pos02 = enemyObj.transform.position + new Vector3(-randX, randY, 0);
        //    if (Vector3.Distance(transform.position, pos01) < Vector3.Distance(transform.position, pos02))
        //    {
        //        targetWorldPos = pos01;
        //        fsm.ChangeState(FSMStates.Move);
        //    }
        //    else
        //    {
        //        targetWorldPos = pos02;
        //        fsm.ChangeState(FSMStates.Move);
        //    }
        //}
    }
}
