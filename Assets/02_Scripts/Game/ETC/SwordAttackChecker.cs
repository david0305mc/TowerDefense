using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttackChecker : MonoBehaviour
{
    private int maxMultiAttackCount;
    private int attackCount;
    private System.Action<Collider2D> attackAction;
    private bool isEnemy;
    public void SetAttackAction(bool _isEnemy, int _maxMultiAttackCount, System.Action<Collider2D> _attackAction)
    {
        maxMultiAttackCount = _maxMultiAttackCount;
        isEnemy = _isEnemy;
        attackAction = _attackAction;
    }
    public void ResetAttackCount()
    {
        attackCount = 0;
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
                    if (maxMultiAttackCount <= attackCount++)
                    {
                        return;
                    }
                    attackAction?.Invoke(collision);
                }
            }
            else
            {
                if (damagable.IsEnemy())
                {
                    if (maxMultiAttackCount <= attackCount++)
                    {
                        return;
                    }
                    Debug.Log("OnTriggerEnter2D");
                    attackAction?.Invoke(collision);
                }
            }
            
        }
    }
}
