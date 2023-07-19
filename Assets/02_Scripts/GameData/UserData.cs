using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UserData : Singleton<UserData>
{
    public Dictionary<int, BaseObjData> baseObjDic;
    public int ShopSelectedItem { get; set; } 
    public void InitData()
    {
        baseObjDic = new Dictionary<int, BaseObjData>();
        ShopSelectedItem = -1;
    }

    public BaseObjData CreateBaseObj(int tid, int x, int y)
    {
        var data = BaseObjData.Create(GameManager.GenerateUID(), tid, x, y);
        baseObjDic.Add(data.UID, data);
        return data;
    }
}
