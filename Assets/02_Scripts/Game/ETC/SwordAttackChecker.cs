using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttackChecker : MonoBehaviour
{
    private System.Action<Collider2D> attackAction;

    public void SetAttackAction(System.Action<Collider2D> _attackAction)
    {
        attackAction = _attackAction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var damagable = collision.GetComponent<Damageable>();
        if (damagable != null)
        {
            attackAction?.Invoke(collision);
        }
    }
}
