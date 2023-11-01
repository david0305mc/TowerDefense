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

    protected virtual void Awake()
    {
        rigidBody2d = GetComponent<Rigidbody2D>();
    }

    public virtual void Shoot(GameObject _targetObj, float _speed)
    {
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
            float dist = Vector2.Distance(srcPos, dstPos);
            float addElapse = Time.deltaTime / dist * speed;
            var moveDelta = lastMoveVector.normalized * addElapse * dist;

            rigidBody2d.transform.position = new Vector2(transform.position.x, transform.position.y) + moveDelta;
            //rigidBody2d.MovePosition(new Vector2(transform.position.x, transform.position.y)  + moveDelta);
            prevPos = transform.position;

            return false;
        }
        else
        {
            float dist = Vector2.Distance(srcPos, dstPos);
            elapse += Time.deltaTime / dist * speed;

            var height = curve.Evaluate(elapse);

            var pos = Vector2.Lerp(srcPos, dstPos, elapse) + new Vector2(0, height);
            rigidBody2d.transform.position = pos;
            //rigidBody2d.MovePosition(pos);
            //rigidBody2d.MoveRotation(GameUtil.LookAt2D(prevPos, pos, GameUtil.FacingDirection.RIGHT));

            lastMoveVector = pos - prevPos;
            targetRotate = GameUtil.LookAt2D(prevPos, pos, GameUtil.FacingDirection.RIGHT);
            rigidBody2d.transform.rotation = targetRotate;
            prevPos = pos;
            //if (pos == new Vector2(transform.position.x, transform.position.y))
            //{
            //    Debug.LogError("pos == new Vector2(transform.position.x, transform.position.y)");
            //}
        }

        return true;
    }

    protected void Dispose()
    {
        Lean.Pool.LeanPool.Despawn(gameObject);
        elapse = 0f;
    }


}
