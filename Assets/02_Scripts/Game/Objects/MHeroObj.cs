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
    }

    private SwordAttackChecker swordAttackChecker;
    private StateMachine<FSMStates, StateDriverUnity> fsm;
    
    private DataManager.Character refData;
    public GameObject targetObj;
    public Vector2 targetoffset;
    private NavMeshPath currNavPath;
    private List<int> blackList;
    private int wayPointIndex;

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
                        damagable.GetDamaged(1);
                        MGameManager.Instance.ShowBoomEffect(0, collision.ClosestPoint(transform.position));

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

            if (fsm.State != FSMStates.Attack)
            {
                return;
            }

            // Attack Ani End
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
    public override bool IsEnemy()
    {
        return false;
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
        agent.isStopped = true;

    }
    void Idle_Update()
    {
        if (MGameManager.Instance.WayPoints.Count > wayPointIndex)
        {
            targetObj = MGameManager.Instance.WayPoints[wayPointIndex].gameObject;
            agent.SetDestination(FixStuckPos(targetObj.transform.position));
            fsm.ChangeState(FSMStates.WaypointMove);
        }
    }

    void WaypointMove_Enter()
    {
        animator.Play("char_01_walk");
        agent.isStopped = false;
    }

    void WaypointMove_Update()
    {
        agent.SetDestination(FixStuckPos(targetObj.transform.position));
        FlipRenderers(agent.velocity.x < 0);
        var detectedObjs = Physics2D.OverlapCircleAll(transform.position, 5, Game.GameConfig.UnitLayerMask);
        if (detectedObjs.Length > 0)
        {
            
            var objLists = detectedObjs.Where(item=>item.GetComponent<MEnemyObj>() != null).Select(item => item.GetComponent<MEnemyObj>());
            var enemyObj = GetNearestEnemy(objLists);

            if (enemyObj != null)
            {
                targetObjUID = enemyObj.UID;
                targetObj = enemyObj.gameObject;
                fsm.ChangeState(FSMStates.DashMove);
                return;
            }
        }

        if (Vector2.Distance(transform.position, targetObj.transform.position) < 0.3f)
        {
            wayPointIndex++;

            if (MGameManager.Instance.WayPoints.Count > wayPointIndex)
            {
                targetObj = MGameManager.Instance.WayPoints[wayPointIndex].gameObject;
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }
        }
    }


    void DashMove_Enter()
    {
        animator.Play("char_01_walk");
        agent.isStopped = false;
        
    }
    
    void DashMove_Update()
    {
        MEnemyObj enemyObj = MGameManager.Instance.GetEnemyObj(targetObjUID);
        if (enemyObj != null)
        {
            FlipRenderers(agent.velocity.x < 0);
            var speed = MGameManager.Instance.GetTileWalkingSpeed(transform.position);
            agent.speed = speed;
            if (!agent.SetDestination(FixStuckPos(targetObj.transform.position)))
            {
                Debug.Log("Error");
            }

            if (Vector2.Distance(transform.position, targetObj.transform.position) < refData.attackrange * 0.5f + 0.01f)
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
        animator.Play("char_01_atk");
        commonDelay = 0;
        LookTarget();
    }

    void Attack_Update()
    {
        commonDelay += Time.deltaTime;
        if (commonDelay >= 0.3f)
        {
            LookTarget();   
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
                    targetObj = enemyObj.gameObject;
                }
                else
                {
                    //targetWorldPos = FixStuckPos(pos02);
                    targetObj = enemyObj.gameObject;
                }

                fsm.ChangeState(FSMStates.DashMove);
            }
        }
    }

    private MEnemyObj GetNearestEnemy(IEnumerable<MEnemyObj> enemyObjs)
    {
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
