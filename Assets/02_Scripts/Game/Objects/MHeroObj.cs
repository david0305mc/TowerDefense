using Cysharp.Threading.Tasks;
using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus;

public class MHeroObj : MBaseObj
{
    public enum FSMStates
    {
        Idle,
        Move,
        WaypointMove,
        AttackMove,
        Attack,
    }

    private SwordAttackChecker swordAttackChecker;
    private StateMachine<FSMStates, StateDriverUnity> fsm;
    
    private DataManager.Character refData;
    public Vector2 targetWorldPos;
    private NavMeshPath currNavPath;
    private List<int> blackList;
    private int wayPointIndex;

    static readonly float agentDrift = 0.0001f; // minimal


    protected override void Awake()
    {
        isEnemy = false;
        blackList = new List<int>();
        currNavPath = new NavMeshPath();
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
                    if (damagable.IsEnemey())
                    {
                        damagable.GetDamaged(1);
                        MGameManager.Instance.ShowBoomEffect(0, collision.ClosestPoint(transform.position));
                    }
                }
            });
        }
        animationLink.SetEvent(() =>
        {
            // Fire
            var enemyData = UserData.Instance.GetEnemyData(targetObjUID);
            if (enemyData != null)
            {
                if (refData.charactertype == CHARACTER_TYPE.ARCHER)
                {
                    MGameManager.Instance.LauchProjectileToEnemy(this, targetObjUID);
                }
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }

        }, ()=> {
            // Ani End
            var enemyData = UserData.Instance.GetEnemyData(targetObjUID);
            if (enemyData == null)
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
    }

    public void StartFSM()
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
        animator.Play("char_01_idle");
        commonDelay = 0f;

    }
    void Idle_Update()
    {
        commonDelay += Time.deltaTime;
        if (commonDelay >= 1f)
        {
            if (MGameManager.Instance.WayPoints.Count > wayPointIndex)
            {
                targetWorldPos = MGameManager.Instance.WayPoints[wayPointIndex].transform.position;
                agent.SetDestination(targetWorldPos);
                fsm.ChangeState(FSMStates.WaypointMove);
            }

            //DetectEnemy();
        }
    }

    void WaypointMove_Enter()
    {
        animator.Play("char_01_walk");
        agent.isStopped = false;
    }

    void WaypointMove_Update()
    {
        FlipRenderers(agent.velocity.x < 0);
        if (Vector2.Distance(transform.position, targetWorldPos) < 0.3f)
        {
            wayPointIndex++;

            if (MGameManager.Instance.WayPoints.Count > wayPointIndex)
            {
                targetWorldPos = MGameManager.Instance.WayPoints[wayPointIndex].transform.position;
                agent.SetDestination(targetWorldPos);
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }
        }
    }

    private void DetectEnemy()
    {
        commonDelay = 0;
        targetObjUID = MGameManager.Instance.GetNearestEnemyObj(transform.position, blackList);

        MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
        if (enemyObj != null)
        {
            if (!agent.CalculatePath(enemyObj.transform.position, currNavPath))
            {
                blackList.Add(targetObjUID);
            }
            else
            {
                float randX = Random.Range(0.5f, 1.5f);
                float randY = Random.Range(-1, 2);

                Vector3 pos01 = enemyObj.transform.position + new Vector3(randX, randY, 0);
                Vector3 pos02 = enemyObj.transform.position + new Vector3(-randX, randY, 0);
                if (Vector3.Distance(transform.position, pos01) < Vector3.Distance(transform.position, pos02))
                {
                    //targetWorldPos = FixStuckPos(pos01);
                    targetWorldPos = enemyObj.transform.position;
                }
                else
                {
                    //targetWorldPos = FixStuckPos(pos02);
                    targetWorldPos = enemyObj.transform.position;
                }

                fsm.ChangeState(FSMStates.Move);
            }
        }
    }

    private Vector3 FixStuckPos(Vector3 _pos)
    {
        if (Mathf.Abs(transform.position.x - _pos.x) < agentDrift)
        {
            var driftPos = _pos + new Vector3(agentDrift, 0f, 0f);
            return driftPos;
        }
        return _pos;
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
        var speed = MGameManager.Instance.GetTileWalkingSpeed(transform.position);
        agent.speed = speed;

        FlipRenderers(agent.velocity.x < 0);
        if (enemyObj != null)
        {
            //FlipRenderers(transform.position.x > enemyObj.transform.position.x);
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
