using Cysharp.Threading.Tasks;
using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MBaseObj : MonoBehaviour, Damageable
{
    public enum FSMStates
    {
        PrevIdle,
        Idle,
        WaypointMove,
        DashMove,
        Attack,
        AttackDelay,
        End,
    }

    [SerializeField] private Slider hpBar;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Rigidbody2D rigidBody2d;
    [SerializeField] protected Transform firePos;
    [SerializeField] protected int tid;
    [SerializeField] protected string state;
    protected UnitBattleData unitData;
    public UnitBattleData UnitData => unitData;

    protected System.Action<AttackData> getDamageAction;
    protected System.Action deadAction;
    private Event deadEvent;
    protected CancellationTokenSource cts;
    protected CancellationTokenSource flashCts;

    private Transform renderRoot;
    protected Animator animator;
    protected AnimationLink animationLink;
    public Vector3 FirePos => firePos.position;

    public GameObject targetObj_Test;

    protected int battleUID;
    public int TID { get { return tid; } }
    public int UID { get { return battleUID; } }

    protected float attackLongDelayCount;
    protected float attackDelay;
    protected float commonDelay;
    protected int targetObjUID;
    protected bool isFixedTarget;

    static readonly float agentDrift = 0.0001f; // minimal

    protected StateMachine<FSMStates, StateDriverUnity> fsm;
    public FSMStates State => fsm.State;

    private SortingGroup sortingGroup;
    protected SwordAttackChecker swordAttackChecker;
    private SpriteRenderer[] spriteRenderers;
    private CircleCollider2D circleCollider;
    private Material originMaterial;
    private List<Color> originColorLists;
    private NavMeshPath currNavPath;
    public Vector3 targetoffset;
    private CancellationTokenSource knockBackCTS;

    public float ColliderRadius => circleCollider.radius;
    private readonly float attackDistMarge = 0.2f;
    private float defaultDirection;
    protected virtual void Awake()
    {
        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }
        knockBackCTS = new CancellationTokenSource();
        sortingGroup = GetComponent<SortingGroup>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        renderRoot = animator.transform;
        defaultDirection = renderRoot.localScale.x;
        animationLink = animator.GetComponent<AnimationLink>();
        swordAttackChecker = GetComponentInChildren<SwordAttackChecker>(true);
        circleCollider = GetComponent<CircleCollider2D>();
        originMaterial = spriteRenderers[0].material;
        originColorLists = new List<Color>();
        hpBar.SetActive(false);

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
            if (unitData.refData.unit_type == UNIT_TYPE.ARCHER)
            {
                if (!UserData.Instance.isUnitDead(targetObjUID, !UnitData.IsEnemy))
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
        fsm.ChangeState(FSMStates.PrevIdle);
    }

    public void SetDeadAction(System.Action _deadAction)
    {
        deadAction = _deadAction;
    }
    public void InitObject(int _battleUID, bool _isEnemy, System.Action<AttackData> _getDamageAction)
    {
        battleUID = _battleUID;
        getDamageAction = _getDamageAction;
        
        if (_isEnemy)
        {
            unitData = UserData.Instance.GetEnemyData(_battleUID);
        }
        else
        {
            unitData = UserData.Instance.GetBattleHeroData(_battleUID);
        }
        hpBar.value = 1f;
        if (swordAttackChecker != null)
        {
            swordAttackChecker.SetAttackAction(_isEnemy, 2, collision =>
            {
                if (fsm.State != FSMStates.Attack)
                {
                    return;
                }
                DoSwordAttack(collision);
            });
        }

        SetBattleMode();
        StartFSM();
    }

    public void SetUIMode(int _sortingOrder)
    {
        if (swordAttackChecker != null)
        {
            swordAttackChecker.IsUIMode = true;
        }
        sortingGroup.sortingLayerName = Game.GameConfig.UILayerName;
        sortingGroup.sortingOrder = _sortingOrder;
        fsm.ChangeState(FSMStates.PrevIdle);
        hpBar.SetActive(false);
        transform.SetScale(200f);
        PlayAni("Idle");
    }

    public void SetBattleMode()
    {
        if (swordAttackChecker != null)
        {
            swordAttackChecker.IsUIMode = false;
        }
        sortingGroup.sortingLayerName = Game.GameConfig.ForegroundLayerName;
        sortingGroup.sortingOrder = 0;
        attackDelay = 0f;
        hpBar.SetActive(false);
        transform.SetScale(1f);
    }

    protected virtual void DoSwordAttack(Collider2D collision)
    {
    }
    protected virtual void DoAttackEnd()
    {
        attackLongDelayCount--;
        if (attackLongDelayCount <= 0)
        {
            //1000 = 1초
            attackDelay = unitData.refUnitGradeData.attacklongdelay * 0.001f;
            attackLongDelayCount = unitData.refUnitGradeData.attackcount;
        }
        else
        {
            attackDelay = unitData.refUnitGradeData.attackshortdelay * 0.001f;
        }

        if (UserData.Instance.isUnitDead(targetObjUID, !UnitData.IsEnemy))
        {
            fsm.ChangeState(FSMStates.Idle);
        }
        else
        {
            MBaseObj opponentUnitObj = MGameManager.Instance.GetUnitObj(targetObjUID, !UnitData.IsEnemy);
            float attackRange = unitData.refUnitGradeData.attackrange * 0.1f;
            if (attackRange < opponentUnitObj.ColliderRadius + this.ColliderRadius)
            {
                attackRange = opponentUnitObj.ColliderRadius + this.ColliderRadius;
            }
            if (Vector2.Distance(transform.position, opponentUnitObj.transform.position + targetoffset) > attackRange + attackDistMarge)
            {
                fsm.ChangeState(FSMStates.DashMove);
            }
            else
            {
                fsm.ChangeState(FSMStates.AttackDelay);
            }
        }
    }

    private void SetAvoidancePriority(int _value)
    {
        if (agent != null)
        {
            agent.avoidancePriority = 1;
        }
    }

    private void EnableAgent(bool _value)
    {
        if (agent != null)
        {
            agent.enabled = _value;
        }
    }

    protected virtual void PrevIdle_Enter()
    {
        state = fsm.State.ToString();
    }

    protected virtual void PrevIdle_Update()
    {
    }

    protected virtual void Idle_Enter()
    {
        PlayAni("Idle");
        StopAgent();
        attackLongDelayCount = unitData.refUnitGradeData.attackcount;
        commonDelay = 0f;
        state = fsm.State.ToString();
        isFixedTarget = false;
        targetoffset = Vector2.zero;
        SetAvoidancePriority(1);
    }
    protected virtual void Idle_Update()
    {
    }

    protected virtual void WaypointMove_Enter()
    {
        SetAvoidancePriority(99);
    }
    protected virtual void WaypointMove_Update()
    {

    }

    protected void StopAgent()
    {
        if (agent == null)
            return;

        if (agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
        }
        rigidBody2d.velocity = Vector3.zero;
        agent.velocity = Vector3.zero;
    }

    protected void ResumeAgent()
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
        }
    }

    protected virtual void DashMove_Enter()
    {
        PlayAni("Walk");
        ResumeAgent();
        state = fsm.State.ToString();
        commonDelay = 0;
        SetAvoidancePriority(99);
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
            FlipRenderers(targetObj.transform.position.x < transform.position.x);
            UpdateAgentSpeed();
            DoAgentMove(targetObj.transform.position + targetoffset);
            float attackRange = unitData.refUnitGradeData.attackrange * 0.1f;
            if (attackRange < targetObj.ColliderRadius + this.ColliderRadius)
            {
                attackRange = targetObj.ColliderRadius + this.ColliderRadius;
            }
            if (Vector2.Distance(transform.position, targetObj.transform.position + targetoffset) < attackRange + attackDistMarge)
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
        StopAgent();
        state = fsm.State.ToString();
        isFixedTarget = true;
        if (attackDelay > 0)
        {
            fsm.ChangeState(FSMStates.AttackDelay);
            return;
        }
        PlayAni("Attack");
        LookTarget();
        SetAvoidancePriority(1);
    }
    protected virtual void Attack_Update()
    {
        LookTarget();
    }

    protected virtual void Attack_Exit()
    {
        if (swordAttackChecker != null)
        {
            swordAttackChecker.ResetAttackCount();
        }
    }

    protected virtual void AttackDelay_Enter()
    {
        PlayAni("AttackIdle");
        state = fsm.State.ToString();
        SetAvoidancePriority(1);
    }

    protected virtual void AttackDelay_Update()
    {
        LookTarget();
        attackDelay -= Time.deltaTime;
        if (attackDelay <= 0f)
        {
            if (UserData.Instance.isUnitDead(targetObjUID, !IsEnemy()))
            {
                fsm.ChangeState(FSMStates.Idle);
            }
            else
            {
                fsm.ChangeState(FSMStates.Attack);
            }
        }
    }
    public void SetEndState()
    {
        fsm.ChangeState(FSMStates.End);
        hpBar.SetActive(false);
    }

    protected virtual void End_Enter()
    {
        PlayAni("Idle");
        StopAgent();
        commonDelay = 0f;
        state = fsm.State.ToString();
    }

    protected virtual void End_Update()
    {
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
        else
        {
            var targetUnitData = UserData.Instance.GetUnitData(targetObjUID, !UnitData.IsEnemy);
            if (targetUnitData.refData.aggroorder == -1)
            {
                SetTargetObject(_attackerUID);
                fsm.ChangeState(FSMStates.DashMove);
            }
        }
    }
    protected void DoAgentMove(Vector3 _pos)
    {
        if (agent == null)
            return;

        if (!agent.isActiveAndEnabled)
            return;

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
    protected virtual void FlipRenderers(bool value)
    {
        if (value)
        {
            renderRoot.localScale = new Vector3(defaultDirection * -1, 1, 1);
        }
        else
        {
            renderRoot.localScale = new Vector3(defaultDirection, 1, 1);
        }
    }

    protected virtual List<MBaseObj> FindUnitListByArea(int _range, bool isTargetEnemy)
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
        return default;
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
        if (agent == null)
            return;
        var speed = MGameManager.Instance.GetTileWalkingSpeed(transform.position);
        agent.speed = UnitData.refUnitGradeData.walkspeed * 0.1f * speed;
    }


    public virtual void GetAttacked(Vector3 attackerPos, int knockBack)
    {
        if (!hpBar.IsActive())
            hpBar.SetActive(true);
        DoFlashEffect();
        UpdateHPBar();
        KnockBack2(attackerPos, knockBack);
        SoundManager.Instance.Play("Sfx/Sfx_001");
    }

    public virtual void GetKilled()
    {
        deadAction?.Invoke();
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
            await UniTask.Delay(100, cancellationToken: flashCts.Token);
            Enumerable.Range(0, spriteRenderers.Length).ToList().ForEach(i =>
            {
                spriteRenderers[i].material = originMaterial;
                spriteRenderers[i].color = originColorLists[i];
            });
        });
    }

    private void KnockBack2(Vector3 attackerPos, int knockBack)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        rigidBody2d.velocity = Vector3.zero;
        Vector3 direction = (transform.position - attackerPos).normalized;
        Vector3 target = transform.position + direction * knockBack * 0.1f;
        Vector3 srcPos = transform.position;

        //넉백 0이어도 발생합니다 ㅠ
        if (knockBack != 0)
        {
            UniTask.Create(async () =>
            {
                EnableAgent(false);
                float elapse = 0f;
                while (Vector3.Distance(transform.position, target) > 0.1f)
                {
                    elapse += Time.deltaTime * 2f;
                    float curveValue = MResourceManager.Instance.KnockBackCurve.Evaluate(elapse);
                    await UniTask.Yield(cancellationToken: cts.Token);
                    rigidBody2d.MovePosition(Vector3.Lerp(srcPos, target, curveValue));
                }
                rigidBody2d.MovePosition(target);
                rigidBody2d.velocity = Vector3.zero;
                EnableAgent(true);
            });
        }
    }
    private void KnockBack(Vector3 attackerPos, int knockBack)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        EnableAgent(false);
        rigidBody2d.velocity = Vector3.zero;
        Vector2 direction = (transform.position - attackerPos).normalized;
        rigidBody2d.AddForce(direction * knockBack, ForceMode2D.Impulse);
        UniTask.Create(async () =>
        {
            await UniTask.Delay(30, cancellationToken: cts.Token);
            EnableAgent(true);
            rigidBody2d.velocity = Vector3.zero;
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
        float randX = Random.Range(0f, 0.31f);
        float randY = Random.Range(-0.1f, 0.1f);

        Vector3 pos01 = new Vector3(randX, randY, 0);
        //pos01 = Vector3.zero;
        Vector3 pos02 = new Vector3(-randX, randY, 0);
        //pos02 = Vector3.zero;

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
}
