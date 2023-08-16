using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttackChecker : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var damagable = collision.GetComponent<Damageable>();
        if (damagable != null)
        {
            damagable.GetDamaged(1);
            MGameManager.Instance.ShowBoomEffect(1, collision.ClosestPoint(transform.position));
        }
    }
}
