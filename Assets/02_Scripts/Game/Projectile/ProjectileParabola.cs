using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileParabola : ProjectileBase
{
    
    [SerializeField] private AnimationCurve curve;
    
    protected override  bool UpdateMissile()
    {
        if (!base.UpdateMissile())
        {
            return false;
        }
        
        elapse += Time.fixedDeltaTime / dist * speed;
        
        var height = curve.Evaluate(elapse);

        var pos = Vector2.Lerp(srcPos, dstPos, elapse) + new Vector2(0, height);
        rigidBody2d.MovePosition(pos);
        rigidBody2d.MoveRotation(GameUtil.LookAt2D(prevPos, pos, GameUtil.FacingDirection.RIGHT));
        lastMoveVector = pos - prevPos;
        prevPos = pos;


        return true;
    }

}