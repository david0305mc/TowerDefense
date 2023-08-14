using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MEnemyObj : MBaseObj
{
    public int UID { get; private set;}
    public int TID;

    private System.Action getDamageAction;
    

    public void InitObject(int _uid, System.Action _getDamageAction)
    {
        UID = _uid;
        getDamageAction = _getDamageAction;
    }

    public override void GetDamaged(int _damage)
    {
        getDamageAction?.Invoke();
    }
}
