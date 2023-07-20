using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using UniRx;

[System.Serializable]
public class LocalData
{
    public ReactiveProperty<long> Gold;
    public SerializableDictionary<int, int> TestDic;
    public SerializableDictionary<int, BaseObjData> baseObjDic;

    public LocalData()
    {
        TestDic = new SerializableDictionary<int, int>();
        Gold = new ReactiveProperty<long>(0);
        baseObjDic = new SerializableDictionary<int, BaseObjData>();
    }

    public void UpdateRefData()
    {
        foreach (var item in baseObjDic)
            item.Value.UpdateRefData();
    }
}

[System.Serializable]
public class BaseObjData
{
    public int UID;
    public int TID;
    public int X;
    public int Y;
    public ObjStatus ObjStatus;
    public GameType.Direction Direction;
    public DataManager.ObjTable RefObjData;
    public void UpdateRefData()
    {
        RefObjData = DataManager.Instance.GetObjTableData(TID);
    }

    public static BaseObjData Create(int uid, int tid, int x, int y)
    {
        BaseObjData data = new BaseObjData();
        data.UID = uid;
        data.TID = tid;
        data.X = x;
        data.Y = y;
        data.ObjStatus = ObjStatus.Idle;
        data.Direction = GameType.Direction.BOTTOM_RIGHT;
        data.UpdateRefData();
        return data;
    }
}