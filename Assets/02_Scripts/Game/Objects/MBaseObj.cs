using Cysharp.Threading.Tasks;
using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class MBaseObj : MonoBehaviour, Damageable
{
    [SerializeField] private Slider hpBar;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Rigidbody2D rigidBody2d;
    [SerializeField] protected Transform firePos;
    [SerializeField] protected int tid;
    
    protected UnitData unitData;
    public UnitData UnitData => unitData;

    protected System.Action getDamageAction;
    protected CancellationTokenSource cts;
    protected Animator animator;
    protected AnimationLink animationLink;
    public Vector3 FirePos => firePos.position;

    protected int uid;
    public int TID { get { return tid; } }
    public int UID { get { return uid; } }

    protected float attackLongDelayCount;
    protected float commonDelay;
    protected int targetObjUID;

    protected virtual void Awake()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        animator = GetComponentInChildren<Animator>();
        animationLink = animator.GetComponent<AnimationLink>();
        if (firePos == default)
        {
            firePos = transform;
        }
    }

    public void InitObject(int _uid, System.Action _getDamageAction)
    {
        uid = _uid;
        getDamageAction = _getDamageAction;
        unitData = UserData.Instance.GetHeroData(_uid);
    }
    public virtual void GetDamaged(int _damage)
    {
        getDamageAction?.Invoke();
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

    public void SetHPBar(float _value)
    {
        hpBar.value = _value;
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

    public virtual bool IsEnemy()
    {
        return false;
    }
}
