using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileGraviryParabola : ProjectileBase
{

    [SerializeField] private AnimationCurve curve;

    public float jumpSpeed;
    public float gravity;
    private float v0;
    
    public override void Shoot(AttackData _attackData, MBaseObj _targetObj, float _speed)
    {
        base.Shoot(_attackData, _targetObj, jumpSpeed);
        v0 = (dstPos - srcPos).y - gravity;
        prevPos = transform.position;
    }
    protected override bool UpdateMissile()
    {
        if (elapse > 1)
        {
            Dispose();
            MGameManager.Instance.ShowBoomEffect(attackData, transform.position);
            return false;
        }
        if (!base.UpdateMissile())
        {
            return false;
        }


        float dist = Mathf.Max(Vector2.Distance(srcPos, dstPos), 3f);
        elapse += Time.fixedDeltaTime / dist * speed;

        var height = curve.Evaluate(elapse);
        Vector2 position = Vector3.Lerp(srcPos, dstPos, elapse); //자연스럽게 이동하기 위해 Lerp사용, percent만큼 이동

        //포물선 운동 : 시작위치 + 초기속도*시간 + 중력*시간제곱
        position.y = srcPos.y + (v0 * elapse) + (gravity * elapse * elapse);

        rigidBody2d.MovePosition(position);
        rigidBody2d.MoveRotation(GameUtil.LookAt2D(prevPos, position, GameUtil.FacingDirection.RIGHT));

        lastMoveVector = position - prevPos;
        prevPos = position;


        return true;
    }
}