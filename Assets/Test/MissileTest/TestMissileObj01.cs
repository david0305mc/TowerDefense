using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMissileObj01 : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    protected Rigidbody2D rigidBody2d;

    protected float elapse;
    protected float speed;
    protected Vector3 prevPos;
    protected GameObject targetObj;

    protected Vector3 srcPos;
    protected Vector3 dstPos;

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
            Dispose();
            return false;
        }

        float dist = Vector2.Distance(srcPos, dstPos);
        elapse += Time.deltaTime / dist * speed;

        var height = curve.Evaluate(elapse);

        var pos = Vector2.Lerp(srcPos, dstPos, elapse) + new Vector2(0, height);
        rigidBody2d.MovePosition(pos);
        rigidBody2d.MoveRotation(GameUtil.LookAt2D(prevPos, pos, GameUtil.FacingDirection.RIGHT));

        prevPos = pos;

        return true;
    }

    protected void Dispose()
    {
        Lean.Pool.LeanPool.Despawn(gameObject);
        elapse = 0f;
    }


}
