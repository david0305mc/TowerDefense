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

    public GameObject targetObj_Test;

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
    private List<Color> originColorLists;
    private NavMeshPath currNavPath;
    public Vector3 targetoffset;

    protected virtual void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        animator = GetComponentInChildren<Animator>();
        animationLink = animator.GetComponent<AnimationLink>();
        swordAttackChecker = GetComponentInChildren<SwordAttackChecker>(true);
        originMaterial = spriteRenderers[0].material;
        originColorLists = new List<Color>();

        Enumerable.Range(0, spriteRenderers.Length).ToList().ForEach(i => {
            originColorLists.Add(spriteRenderers[i].color);
        });
        
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

            DoAttackEnd();
        });
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

    protected virtual void DoSwordAttack(Collider2D collision)
    {
    }
    protected virtual void DoAttackEnd()
    {
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
            if (Vector2.Distance(transform.position, opponentUnitObj.transform.position + targetoffset) > unitData.refUnitGradeData.attackrange * 0.1f + 0.01f)
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
    }

    protected virtual void Idle_Enter()
    {
        PlayAni("Idle");
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        attackLongDelayCount = unitData.refUnitGradeData.attackcount;
        commonDelay = 0f;
        state = fsm.State.ToString();
        isFixedTarget = false;
        targetoffset = Vector2.zero;
    }
    protected virtual void Idle_Update()
    {
    }

    protected virtual void WaypointMove_Enter()
    {

    }
    protected virtual void WaypointMove_Update()
    {
          
    }

    protected virtual void DashMove_Enter()
    {
        PlayAni("Walk");
        agent.isStopped = false;
        state = fsm.State.ToString();
        commonDelay = 0;
    }
    protected virtual void DashMove_Update()
    {
        if (!isFixedTarget)
        {
            commonDelay -= Time.deltaTime;
            if (commonDelay <= 0)
            {
                commonDelay = 0.01f;
                var targetLists = FindUnitListByArea(unitData.refData.checkrange, !IsEnemy());
                if (targetLists != null && targetLists.Count > 0)
                {
                    MBaseObj findTargetObj = FindNearestTargetByAggroOrder(targetLists);
                    if (findTargetObj != null)
                    {
                        SetTargetObject(findTargetObj.UID);
                    }
                }
            }
        }

        MBaseObj targetObj = MGameManager.Instance.GetUnitObj(targetObjUID, !IsEnemy());
        if (targetObj != null)
        {
            FlipRenderers(agent.velocity.x < 0);
            UpdateAgentSpeed();
            DoAgentMove(targetObj.transform.position + targetoffset);

            if (Vector2.Distance(transform.position, targetObj.transform.position + targetoffset) < unitData.refUnitGradeData.attackrange * 0.1f + 0.01f)
            {
                fsm.ChangeState(FSMStates.Attack);
            }
        }
        else
        {
            fsm.ChangeState(FSMStates.Idle);
        }
    }

    protected virtual void Attack_Enter()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        PlayAni("Attack");
        LookTarget();
        state = fsm.State.ToString();
        commonDelay = 0f;
        isFixedTarget = true;
    }
    protected virtual void Attack_Update()
    {
        LookTarget();
    }

    protected virtual void AttackDelay_Enter()
    {
        PlayAni("Idle");
    }

    protected virtual void AttackDelay_Update()
    {
        LookTarget();
        commonDelay -= Time.deltaTime;
        if (commonDelay <= 0f)
        {
            if (UserData.Instance.GetUnitData(targetObjUID, !IsEnemy()) != null)
            {
                fsm.ChangeState(FSMStates.Attack);
            }
            else
            {
                fsm.ChangeState(FSMStates.Idle);
            }
        }
    }


    protected virtual void Update()
    {
        fsm.Driver.Update.Invoke();
    }

    private void OnDisable()
    {
        cts?.Cancel();
        flashCts?.Cancel();

        Enumerable.Range(0, spriteRenderers.Length).ToList().ForEach(i =>
        {
            spriteRenderers[i].material = originMaterial;
            spriteRenderers[i].color = originColorLists[i];
        });
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        flashCts?.Cancel();
        flashCts?.Dispose();
    }

    public virtual void StartFSM()
    {
        fsm.ChangeState(FSMStates.Idle);
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
        if (!isFixedTarget)
        {
            SetTargetObject(_attackerUID);
            fsm.ChangeState(FSMStates.DashMove);
        }
    }
    protected void DoAgentMove(Vector3 _pos)
    {
        Vector3 fixedPos = GetFixedStuckPos(_pos);
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(fixedPos, path))
        {
            agent.SetPath(path);
        }
        else
        {
            Debug.Log("path not found");
            Vector3 samplePos = SamplePosition(fixedPos);
            if (samplePos != Game.GameConfig.PositiveInfinityVector)
            {
                if (agent.CalculatePath(samplePos, path))
                {
                    agent.SetPath(path);
                }
            }
            //agent.SetDestination(fixedPos);
        }
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

    protected List<MBaseObj> FindUnitListByArea(int _range, bool isTargetEnemy)
    {
        var detectedObjs = Physics2D.OverlapCircleAll(transform.position, _range * 0.1f, Game.GameConfig.UnitLayerMask);
        if (detectedObjs.Length > 0)
        {
            return detectedObjs.Where(item =>
            {
                MBaseObj baseObj = item.GetComponent<MBaseObj>();
                if (baseObj != null)
                {
                    if (isTargetEnemy)
                    {
                        return baseObj.IsEnemy();
                    }
                    else
                    {
                        return !baseObj.IsEnemy();
                    }
                }
                else
                {
                    return false;
                }
            }).Select(item => item.GetComponent<MBaseObj>()).ToList();
        }
        return null;
    }
    protected MBaseObj FindNearestTargetByAggroOrder(IEnumerable<MBaseObj> targetObjs)
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
            Enumerable.Range(0, spriteRenderers.Length).ToList().ForEach(i =>
            {
                spriteRenderers[i].material = originMaterial;
                spriteRenderers[i].color = originColorLists[i];
            });
        });
    }
    protected void LookTarget()
    {
        MBaseObj targetObj = MGameManager.Instance.GetUnitObj(targetObjUID, !IsEnemy());
        if (targetObj != null)
        {
            FlipRenderers(targetObj.transform.position.x < transform.position.x);
        }
    }

    public virtual bool IsEnemy()
    {
        return UnitData.IsEnemy;
    }

    protected virtual bool SetTargetObject(int _uid)
    {
        if (targetObjUID == _uid)
        {
            return false;
        }
        MBaseObj targetObj = MGameManager.Instance.GetUnitObj(_uid, !IsEnemy());
        float randX = Random.Range(0.5f, 1.5f);
        float randY = Random.Range(-1.5f, 1.5f);

        Vector3 pos01 = new Vector3(randX, randY, 0);
        Vector3 pos02 = new Vector3(-randX, randY, 0);

        var samplePos01 = pos01 + new Vector3(targetObj.transform.position.x, targetObj.transform.position.y, 0);
        var samplePos02 = pos02 + new Vector3(targetObj.transform.position.x, targetObj.transform.position.y, 0);

        //var samplePos01 = SamplePosition(pos01 + new Vector3(targetObj.transform.position.x, targetObj.transform.position.y, 0));
        //var samplePos02 = SamplePosition(pos02 + new Vector3(targetObj.transform.position.x, targetObj.transform.position.y, 0));
        //if (samplePos01  == Game.GameConfig.PositiveInfinityVector && samplePos02 == Game.GameConfig.PositiveInfinityVector)
        //{
        //    Debug.Log("not available TargetPos");
        //    return false;
        //}

        if (Vector2.Distance(samplePos01, transform.position) < Vector2.Distance(samplePos02, transform.position))
        {
            targetoffset = pos01;
        }
        else
        {
            targetoffset = pos02;
        }
        targetObj_Test = targetObj.gameObject;
        targetObjUID = _uid;
        return true;
    }

    private float CalcPathLength(Vector3 _targetPos)
    {
        NavMeshPath _path = new NavMeshPath();
        if (!agent.CalculatePath(_targetPos, _path))
        {
            return float.MaxValue;
        }
        
        Vector3[] _wayPoint = new Vector3[_path.corners.Length + 2];

        _wayPoint[0] = transform.position;
        _wayPoint[_path.corners.Length + 1] = _targetPos;

        float _pathLength = 0;  // 경로 길이를 더함
        for (int i = 0; i < _path.corners.Length; i++)
        {
            _wayPoint[i + 1] = _path.corners[i];
            _pathLength += Vector3.Distance(_wayPoint[i], _wayPoint[i + 1]);
        }

        return _pathLength;
    }

    private Vector3 SamplePosition(Vector3 _pos)
    {
        //transform.position + Random.insideUnitSphere * maxRange,
        NavMeshHit navMeshHit;
        float maxRange = 10f;
        bool foundPosition =
            NavMesh.SamplePosition(
                _pos,
                out navMeshHit,
                maxRange,
                NavMesh.AllAreas);

        if (foundPosition)
        {
            return navMeshHit.position;
        }

        return Game.GameConfig.PositiveInfinityVector;
    }

    private void ResetTrigger()
    {
        foreach (var p in animator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(p.name);
            }
        }
    }
}
