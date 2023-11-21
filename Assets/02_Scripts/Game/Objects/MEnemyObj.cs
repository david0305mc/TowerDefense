using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AI;

public class MEnemyObj : MBaseObj
{
    [SerializeField] private GameObject devilHitedIcon;
    [SerializeField] private bool isEnemyBoss;
    public bool IsEnemyBoss => isEnemyBoss;

    [SerializeField] private float rangeCheckForEditor = 3f;

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = new Color(1.0f, 0, 0, 0.3f);
    //    Gizmos.matrix = transform.localToWorldMatrix;
    //    Gizmos.DrawSphere(Vector3.zero, rangeCheckForEditor);
    //}

    protected override void Awake()
    {
        base.Awake();
        devilHitedIcon = hpBar.transform.Find("DevilHittedIcon").gameObject;
        devilHitedIcon?.SetActive(false);
    }

    public override void StartFSM()
    {
        base.StartFSM();
    }
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;

    //    Gizmos.DrawCube(transform.position - new Vector3(ColliderRadius + MGameManager.Instance.AttackRangeW * 0.5f, 0, 0), new Vector3(MGameManager.Instance.AttackRangeW, MGameManager.Instance.AttackRangeH, 0));
    //    Gizmos.DrawCube(transform.position - new Vector3(-(ColliderRadius + MGameManager.Instance.AttackRangeW * 0.5f), 0, 0), new Vector3(MGameManager.Instance.AttackRangeW, MGameManager.Instance.AttackRangeH, 0));
    //}
    protected override void Idle_Enter()
    {
        base.Idle_Enter();
        detectDelay = Random.Range(0, 2f);
        if (MGameManager.Instance.CurrStageObj.devileCastleSpawnPoint != null)
        {
            targetWayPoint = MGameManager.Instance.CurrStageObj.devileCastleSpawnPoint.position;
            fsm.ChangeState(FSMStates.WaypointMove);
        }
    }
    protected override void Idle_Update()
    {
        detectDelay -= Time.deltaTime;
        if (detectDelay <= 0)
        {
            detectDelay = Random.Range(0, 2f);
            DetectHero();
        }
    }
    protected override void WaypointMove_Enter()
    {
        PlayAni("Walk");
        ResumeAgent();
        state = fsm.State.ToString();
    }
    protected override void WaypointMove_Update()
    {
        UpdateAgentSpeed();
        DoAgentMove(targetWayPoint);
        FlipRenderers(targetWayPoint.x < transform.position.x);

        var targetLists = FindUnitListByArea(unitData.refData.checkrange, false);
        if (targetLists.Count > 0)
        {
            var enemyObj = FindNearestTargetByAggroOrder(targetLists);
            if (enemyObj != null)
            {
                if (SetTargetObject(enemyObj.UID))
                {
                    fsm.ChangeState(FSMStates.DashMove);
                    return;
                }
            }
        }

        if (Vector2.Distance(transform.position, targetWayPoint) < 0.3f)
        {
            fsm.ChangeState(FSMStates.Idle);
        }
    }
    protected override void DoSwordAttack(Collider2D collision)
    {
        var damagable = collision.GetComponent<Damageable>();
        if (damagable != null)
        {
            if (!damagable.IsEnemy())
            {
                var attackData = new AttackData(unitData.battleUID, this.unitData.tid, this.unitData.attackDamage, unitData.grade, !UnitData.IsEnemy);
                MGameManager.Instance.ShowBoomEffect(attackData, collision.ClosestPoint(transform.position));
                damagable.GetDamaged(attackData);
            }
        }
    }

    public override void DoAggro(int _attackerUID)
    {
        if (_attackerUID == Game.GameConfig.UserObjectUID)
        {
            return;
        }
        base.DoAggro(_attackerUID);
    }


    private Vector2 GetRandomWayPoint(float _dist)
    {
        for (int i = 0; i < 10; i++)
        {
            var pos = (Vector2)transform.position + Random.insideUnitCircle * _dist;
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(pos, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    return pos;
                }
            }
        }
        return transform.position;
    }

    public virtual void DoUserSkillReact()
    {
        if (fsm.State == FSMStates.Idle || fsm.State == FSMStates.PrevIdle)
        {
            targetWayPoint = GetRandomWayPoint(3f);
            fsm.ChangeState(FSMStates.WaypointMove);
            UniTask.Create(async () =>
            {
                devilHitedIcon?.SetActive(true);
                await UniTask.WaitForSeconds(2f, cancellationToken: this.GetCancellationTokenOnDestroy());
                devilHitedIcon?.SetActive(false);
            });
        }
    }

    private void DetectHero()
    {
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
    }

    protected override void DashMove_Update()
    {
        base.DashMove_Update();
    }

    protected override void Attack_Enter()
    {
        base.Attack_Enter();
    }
    protected override void Attack_Update()
    {
        base.Attack_Update();
    }

    protected override void AttackDelay_Enter()
    {
        base.AttackDelay_Enter();
    }

    protected override void AttackDelay_Update()
    {
        base.AttackDelay_Update();
    }
}
