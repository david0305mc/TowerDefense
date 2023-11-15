using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
        if (cfxrEffect != null)
        {
            cfxrEffect.EndAction = _endAction;
        }
        else
        {
            UniTask.Create(async () =>
            {
                await UniTask.WaitForSeconds(3f);
            });
        }
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
