using Cysharp.Threading.Tasks;
using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class MHeroObj : MBaseObj
{
    public enum FSMStates
    {
        Idle,
        Move,
        AttackMove,
        Attack,
    }
    StateMachine<FSMStates, StateDriverUnity> fsm;

    [SerializeField] private Animator animator;
    [SerializeField] private AnimationLink animationLink;
    [SerializeField] private NavMeshAgent agent;
    private SwordAttackChecker swordAttackChecker;

    [SerializeField] private int tid;
    public int TID { get { return tid; } }

    private float commonDelay;
    private int UID;
    public System.Action getDamageAction { get; private set; }

    private DataManager.Character refData;
    Vector2 targetWorldPos;
    int targetObjUID;
    

    void Awake()
    {
        swordAttackChecker = GetComponentInChildren<SwordAttackChecker>(true);
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
        animationLink.SetFireEvent(() =>
        {
            var enemyData = UserData.Instance.GetEnemyData(targetObjUID);
            if (enemyData != null)
            {
                if (refData.charactertype == CHARACTER_TYPE.ARCHER)
                {
                    MGameManager.Instance.LauchProjectile(this, targetObjUID);
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
        UID = _uid;
        getDamageAction = _getDamageAction;
        refData = DataManager.Instance.GetCharacterData(TID);
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
        targetObjUID = MGameManager.Instance.GetNearestEnemyObj(transform.position);
        MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
        if (enemyObj != null)
        {
            float randX = Random.Range(0.5f, 1.5f);
            float randY = Random.Range(-1, 2);

            Vector3 pos01 = enemyObj.transform.position + new Vector3(randX, randY, 0);
            Vector3 pos02 = enemyObj.transform.position + new Vector3(-randX, randY, 0);
            if (Vector3.Distance(transform.position, pos01) < Vector3.Distance(transform.position, pos02))
            {
                targetWorldPos = pos01;
                fsm.ChangeState(FSMStates.Move);
            }
            else
            {
                targetWorldPos = pos02;
                fsm.ChangeState(FSMStates.Move);
            }
        }
    }

    void Move_Enter()
    {
        animator.Play("char_01_walk");
        agent.isStopped = false;
        agent.SetDestination(targetWorldPos);
    }
    void Move_Update()
    {
        MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
        if (enemyObj != null)
        {
            FlipRenderers(transform.position.x > enemyObj.transform.position.x);
            if (Vector2.Distance(transform.position, targetWorldPos) < refData.attackrange * 0.1f + 0.01f)
            {
                agent.isStopped = true;
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
        animator.Play("char_01_atk");
        commonDelay = 0;
    }

    void Attack_Update()
    {
        
    }
}
