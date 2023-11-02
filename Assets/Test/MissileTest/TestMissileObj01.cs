using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestMissileObj01 : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    protected Rigidbody2D rigidBody2d;

    protected float elapse;
    protected float speed;
    protected Vector2 prevPos;
    protected GameObject targetObj;

    protected Vector3 srcPos;
    protected Vector3 dstPos;
    private Quaternion targetRotate;
    private Vector2 lastMoveVector;

    private int multiTargetCount;
    private float lifeTime;
    private float afterHitLifeTime;
    private bool toBeDisposal;

    protected virtual void Awake()
    {
        rigidBody2d = GetComponent<Rigidbody2D>();
    }

    public virtual void Shoot(GameObject _targetObj, float _speed)
    {
        lifeTime = 10f;
        targetObj = _targetObj;
        multiTargetCount = 0;
        srcPos = transform.position;
        elapse = 0f;
        speed = _speed;
        prevPos = srcPos;
        toBeDisposal = false;
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
        if (toBeDisposal)
        {
            afterHitLifeTime -= Time.fixedDeltaTime;
            if (afterHitLifeTime <= 0)
            {
                Dispose();
                return false;
            }
        }

        if (targetObj != null)
        {
            dstPos = targetObj.transform.position;
        }

        if (elapse >= 1)
        {
            float dist = Vector2.Distance(srcPos, dstPos);
            float addElapse = (Time.fixedDeltaTime / dist) * speed;
            var moveDelta = lastMoveVector.normalized * addElapse * dist;

            //rigidBody2d.transform.position = new Vector2(transform.position.x, transform.position.y) + moveDelta;
            rigidBody2d.MovePosition(new Vector2(transform.position.x, transform.position.y) + moveDelta);
            prevPos = transform.position;

            return false;
        }
        else
        {
            float dist = Vector2.Distance(srcPos, dstPos);
            elapse += Time.fixedDeltaTime / dist * speed;

            var height = curve.Evaluate(elapse);

            var pos = Vector2.Lerp(srcPos, dstPos, elapse) + new Vector2(0, height);
            
            rigidBody2d.MovePosition(pos);
            rigidBody2d.MoveRotation(GameUtil.LookAt2D(prevPos, pos, GameUtil.FacingDirection.RIGHT));

            lastMoveVector = pos - prevPos;
            prevPos = pos;
            if (pos == new Vector2(transform.position.x, transform.position.y))
            {
                Debug.LogError("pos == new Vector2(transform.position.x, transform.position.y)");
            }
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
        if (multiTargetCount == 0)
        {
            toBeDisposal = true;
            afterHitLifeTime = 0.1f;
        }

        multiTargetCount++;
        if (multiTargetCount > 10)
        {
            Dispose();
        }

        Debug.LogError("Hit");
    }
}
