using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{

    private TrailRenderer trailRenderer;
    protected Rigidbody2D rigidBody2d;

    protected float elapse;
    protected float speed;
    protected Vector3 prevPos;
    protected AttackData attackData;
    protected MBaseObj targetObj;

    protected Vector3 srcPos;
    protected Vector3 dstPos;

    private bool isDisposed;
    protected virtual void Awake()
    {
        rigidBody2d = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public virtual void Shoot(AttackData _attackData, MBaseObj _targetObj, float _speed)
    {
        attackData = _attackData;
        targetObj = _targetObj;
        
        srcPos = transform.position;
        elapse = 0f;
        speed = _speed;
        prevPos = srcPos;
        isDisposed = false;
        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }
    }
    private void Update()
    {
        UpdateMissile();
    }

    protected virtual bool UpdateMissile()
    {
        if (targetObj != null)
        {
            dstPos = targetObj.transform.position;
        }

        if (elapse >= 1)
        {
            Dispose();
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
        var damagable = collision.GetComponent<Damageable>();
        if (damagable != null)
        {
            if (damagable.IsEnemy())
            {
                if (targetObj.IsEnemy())
                {
                    MGameManager.Instance.ShowBoomEffect(attackData, collision.ClosestPoint(transform.position));
                    MGameManager.Instance.DoAreaAttack(attackData, collision.ClosestPoint(transform.position));
                    damagable.GetDamaged(attackData);
                    Dispose();
                }
            }
            else
            {
                if (!targetObj.IsEnemy())
                {
                    MGameManager.Instance.ShowBoomEffect(attackData, collision.ClosestPoint(transform.position));
                    MGameManager.Instance.DoAreaAttack(attackData, collision.ClosestPoint(transform.position));
                    damagable.GetDamaged(attackData);
                    Dispose();
                }
            }
        }
    }
}
