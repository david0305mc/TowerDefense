using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{

    private TrailRenderer trailRenderer;
    protected Rigidbody2D rigidBody2d;

    protected float elapse;
    protected float speed;
    protected Vector2 prevPos;
    protected AttackData attackData;
    protected MBaseObj targetObj;

    protected Vector2 srcPos;
    protected Vector2 dstPos;
    protected Vector2 lastMoveVector;
    protected float dist;

    private bool isDisposed;
    private bool toBeDiposal;
    private int targetAttackCount;
    private float lifeTime;
    private float afterHitLifeTime;
    private bool isNontarget;
    private float moveDist;

    protected virtual void Awake()
    {
        rigidBody2d = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public virtual void Shoot(AttackData _attackData, MBaseObj _targetObj, float _speed)
    {
        attackData = _attackData;
        targetObj = _targetObj;
        
        var unitGradeInfo = DataManager.Instance.GetUnitGrade(attackData.attackerTID, attackData.grade);
        targetAttackCount = unitGradeInfo.multiattackcount;
        var projectileInfo = DataManager.Instance.GetProjectileInfoData(unitGradeInfo.projectileid);
        lifeTime = projectileInfo.lifetime * 0.001f;
        afterHitLifeTime = projectileInfo.afterhitlifetime * 0.001f;
        isNontarget = projectileInfo.nontarget == 1;
        toBeDiposal = false;
        
        dstPos = targetObj.transform.position;
        srcPos = transform.position;
        dist = Vector2.Distance(srcPos, dstPos);
        moveDist = Vector2.Distance(srcPos, dstPos);
        elapse = 0f;
        speed = _speed;
        prevPos = srcPos;
        isDisposed = false;
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }
    private void FixedUpdate()
    {
        UpdateMissile();
    }

    protected virtual bool UpdateMissile()
    {
        lifeTime -= Time.fixedDeltaTime;
        if (lifeTime <= 0)
        {
            Dispose();
            return false;
        }
        if (toBeDiposal)
        {
            afterHitLifeTime -= Time.fixedDeltaTime;
            if (afterHitLifeTime <= 0)
            {
                Dispose();
                return false;
            }
        }

        if (targetObj != null && !isNontarget)
        {
            dstPos = targetObj.transform.position;
        }

        if (elapse >= 1)
        {
            float addElapse = (Time.fixedDeltaTime / moveDist) * speed;
            var moveDelta = lastMoveVector.normalized * addElapse * moveDist;

            //rigidBody2d.transform.position = new Vector2(transform.position.x, transform.position.y) + moveDelta;
            rigidBody2d.MovePosition(new Vector2(transform.position.x, transform.position.y) + moveDelta);
            prevPos = transform.position;
            return false;
        }
        return true;
    }

    protected void Dispose()
    {
        if (!isDisposed)
        {
            Lean.Pool.LeanPool.Despawn(gameObject);
            elapse = 0f;
            isDisposed = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Damageable damagable = collision.GetComponent<Damageable>();
        if (damagable != null)
        {
            if (damagable.IsEnemy())
            {
                if (targetObj.IsEnemy())
                {
                    Hit(collision, damagable);
                }
            }
            else
            {
                if (!targetObj.IsEnemy())
                {
                    Hit(collision, damagable);
                }
            }
        }
    }
    private void Hit(Collider2D collision, Damageable damagable)
    {
        if (!toBeDiposal)
        {
            toBeDiposal = true;
        }

        MGameManager.Instance.ShowBoomEffect(attackData, collision.ClosestPoint(transform.position));
        MGameManager.Instance.DoAreaAttack(attackData, collision.ClosestPoint(transform.position));
        damagable.GetDamaged(attackData);

        targetAttackCount--;
        if (targetAttackCount <= 0)
        {
            Dispose();
        }
    }
}
