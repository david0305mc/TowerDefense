using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Damageable
{
    public void GetDamaged(AttackData _attackData);
    public bool IsEnemy();
}