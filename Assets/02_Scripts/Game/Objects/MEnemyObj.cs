using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MEnemyObj : MBaseObj
{
    public int UID { get; private set;}
    public int TID;

    public void InitObject(int _uid)
    {
        UID = _uid;
    }

}
