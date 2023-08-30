using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    private CartoonFX.CFXR_Effect cfxrEffect;
    private AttackData attackData;
    private void Awake()
    {
        cfxrEffect = GetComponent<CartoonFX.CFXR_Effect>();
    }

    public void SetData(AttackData _attackData, System.Action _endAction)
    {
        attackData = _attackData;
        cfxrEffect.EndAction = _endAction;
    }

    private void Dispose()
    {
        Lean.Pool.LeanPool.Despawn(gameObject);
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    var damagable = collision.GetComponent<Damageable>();
    //    if (damagable != null)
    //    {
    //        if (damagable.IsEnemy())
    //        {
    //            if (attackData.attackToEnemy)
    //            {
    //                damagable.GetDamaged(attackData);
    //                Dispose();
    //            }
    //        }
    //        else
    //        {
    //            if (!attackData.attackToEnemy)
    //            {
    //                damagable.GetDamaged(attackData);
    //                Dispose();
    //            }
    //        }
    //    }
    //}
}
