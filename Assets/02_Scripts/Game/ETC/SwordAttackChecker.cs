using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttackChecker : MonoBehaviour
{
    private System.Action<Collider2D> attackAction;
    private bool isEnemy;
    public void SetAttackAction(bool _isEnemy, System.Action<Collider2D> _attackAction)
    {
        isEnemy = _isEnemy;
        attackAction = _attackAction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var damagable = collision.GetComponent<Damageable>();
        if (damagable != null)
        {
            if (isEnemy)
            {
                if (!damagable.IsEnemy())
                {
                    attackAction?.Invoke(collision);
                }
            }
            else
            {
                if (damagable.IsEnemy())
                {
                    attackAction?.Invoke(collision);
                }
            }
            
        }
    }
}
