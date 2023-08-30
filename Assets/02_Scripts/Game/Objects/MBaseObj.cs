using Cysharp.Threading.Tasks;
using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class MBaseObj : MonoBehaviour, Damageable
{
    public enum FSMStates
    {
        Idle,
        WaypointMove,
        DashMove,
        Attack,
        AttackDelay,
    }

    [SerializeField] private Slider hpBar;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Rigidbody2D rigidBody2d;
    [SerializeField] protected Transform firePos;
    [SerializeField] protected int tid;

    [SerializeField] protected string state;
    protected UnitData unitData;
    public UnitData UnitData => unitData;

    protected System.Action<AttackData> getDamageAction;
    protected CancellationTokenSource cts;
    protected CancellationTokenSource flashCts;

    protected Animator animator;
    protected AnimationLink animationLink;
    public Vector3 FirePos => firePos.position;

    protected int uid;
    public int TID { get { return tid; } }
    public int UID { get { return uid; } }

    protected float attackLongDelayCount;
    protected float commonDelay;
    protected int targetObjUID;
    protected bool isFixedTarget;

    static readonly float agentDrift = 0.0001f; // minimal

    protected StateMachine<FSMStates, StateDriverUnity> fsm;
    public FSMStates State => fsm.State;

    protected SwordAttackChecker swordAttackChecker;
    private SpriteRenderer[] spriteRenderers;
    private Material originMaterial;
    private Color originColor;
    protected virtual void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        animator = GetComponentInChildren<Animator>();
        animationLink = animator.GetComponent<AnimationLink>();
        swordAttackChecker = GetComponentInChildren<SwordAttackChecker>(true);
        originMaterial = spriteRenderers[0].material;
        originColor = spriteRenderers[0].color;
        if (firePos == default)
        {
            firePos = transform;
        }

        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
        animationLink.SetEvent(() =>
        {
            if (fsm.State != FSMStates.Attack)
            {
                return;
            }
            // Fire Only For Projectile
            var opponentUnitData = UserData.Instance.GetUnitData(targetObjUID, !UnitData.IsEnemy);
            if (opponentUnitData != null)
            {
                if (this.unitData.refData.unit_type == UNIT_TYPE.ARCHER)
                {
                    MGameManager.Instance.LauchProjectile(this, targetObjUID);
                }
            }

        }, () => {

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

            var opponentUnitData = UserData.Instance.GetUnitData(targetObjUID, !UnitData.IsEnemy);
            if (opponentUnitData != null)
            {
                MBaseObj opponentUnitObj = MGameManager.Instance.GetUnitObj(targetObjUID, !UnitData.IsEnemy);
                if (Vector2.Distance(transform.position, opponentUnitObj.transform.position) > unitData.refUnitGradeData.attackrange * 0.1f + 0.01f)
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

    protected virtual void DoSwordAttack(Collider2D collision)
    {
    
    }

    private void OnDisable()
    {
        cts?.Cancel();
        flashCts?.Cancel();
        foreach (var item in spriteRenderers)
        {
            item.material = originMaterial;
            item.color = originColor;
        }
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        flashCts?.Cancel();
        flashCts?.Dispose();
    }

    public void InitObject(int _uid, bool _isEnemy, System.Action<AttackData> _getDamageAction)
    {
        uid = _uid;
        getDamageAction = _getDamageAction;
        if (_isEnemy)
        {
            unitData = UserData.Instance.GetEnemyData(_uid);
        }
        else
        {
            unitData = UserData.Instance.GetHeroData(_uid);
        }
        hpBar.value = 1F;
        if (swordAttackChecker != null)
        {
            swordAttackChecker.SetAttackAction(_isEnemy, collision =>
            {
                if (fsm.State != FSMStates.Attack)
                {
                    return;
                }
                DoSwordAttack(collision);
            });
        }
        StartFSM();
    }
    public virtual void StartFSM()
    {
    }

    public virtual void GetDamaged(AttackData _attackData)
    {
        getDamageAction?.Invoke(_attackData);
    }

    protected void PlayAni(string str)
    {
        //ResetTrigger();
        //animator.SetTrigger(str);

        animator.Play(str);
        animator.Update(0);
    }
    public void MoveTo(Vector3 targetPos)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        UniTask.Create(async () => {

            while (true)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
                if (Vector2.Distance(transform.position, targetPos) < 0.1f)
                {
                    cts.Cancel();
                    break;
                }
                await UniTask.WaitForFixedUpdate(cancellationToken: cts.Token);
            }
        });
    }
    protected Vector3 GetFixedStuckPos(Vector3 _pos)
    {
        if (Mathf.Abs(transform.position.x - _pos.x) < agentDrift)
        {
            _pos = _pos + new Vector3(agentDrift, 0f, 0f);
        }
        return new Vector3(_pos.x, _pos.y, 0);
    }
    public virtual void DoAggro(int _attackerUID)
    { 
        
    }
    public void UpdateHPBar()
    {
        hpBar.value = UnitData.hp / (float)UnitData.refUnitGradeData.hp;
    }
    protected void FlipRenderers(bool value)
    {
        if (value)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    protected MBaseObj GetNearestTargetByAggro(IEnumerable<MBaseObj> targetObjs)
    {
        if (targetObjs.Count() == 0)
            return null;

        int maxAggroOrder = targetObjs.Max(item => item.UnitData.refData.aggroorder);
        targetObjs = targetObjs.Where(item => item.UnitData.refData.aggroorder == maxAggroOrder);
        float nearestDist = float.MaxValue;
        MBaseObj nearestTarget = default;
        foreach (var item in targetObjs)
        {
            float dist = Vector2.Distance(transform.position, item.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestTarget = item;
            }
        }
        return nearestTarget;
    }
    protected void UpdateAgentSpeed()
    {
        var speed = MGameManager.Instance.GetTileWalkingSpeed(transform.position);
        agent.speed = UnitData.refUnitGradeData.walkspeed * 0.1f * speed;
    }

    public void DoFlashEffect()
    {
        flashCts?.Cancel();
        flashCts = new CancellationTokenSource();
        foreach (var item in spriteRenderers)
        {
            item.material = MResourceManager.Instance.FlashMaterial;
            item.color = MResourceManager.Instance.FlashColor;
        }

        UniTask.Create(async () =>
        {
            await UniTask.Delay(300, cancellationToken: flashCts.Token);
            foreach (var item in spriteRenderers)
            {
                item.material = originMaterial;
                item.color = originColor;
            }
        });
    }
    public virtual bool IsEnemy()
    {
        return false;
    }
}
