using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    protected Rigidbody2D rigidBody2d;

    protected float elapse;
    protected float speed;
    protected Vector3 prevPos;
    protected AttackData attackData;
    protected MBaseObj targetObj;

    protected Vector3 srcPos;
    protected Vector3 dstPos;

    protected virtual void Awake()
    {
        rigidBody2d = GetComponent<Rigidbody2D>();
    }

    public virtual void Shoot(AttackData _attackData, MBaseObj _targetObj, float _speed)
    {
        attackData = _attackData;
        targetObj = _targetObj;
        
        srcPos = transform.position;
        elapse = 0f;
        speed = _speed;
        prevPos = srcPos;
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
        Lean.Pool.LeanPool.Despawn(gameObject);
        elapse = 0f;
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
                    damagable.GetDamaged(attackData);
                    MGameManager.Instance.ShowBoomEffect(0, collision.ClosestPoint(transform.position));
                    Dispose();
                }
            }
            else
            {
                if (!targetObj.IsEnemy())
                {
                    damagable.GetDamaged(attackData);
                    MGameManager.Instance.ShowBoomEffect(0, collision.ClosestPoint(transform.position));
                    Dispose();
                }
            }
        }
    }
}
