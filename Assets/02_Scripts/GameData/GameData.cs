using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using UniRx;

[System.Serializable]
public class LocalData
{
    public int uidSeed;
    public ReactiveProperty<long> Gold;
    public SerializableDictionary<int, BaseObjData> BaseObjDic;

    public LocalData()
    {
        uidSeed = 0;
        Gold = new ReactiveProperty<long>(0);
        BaseObjDic = new SerializableDictionary<int, BaseObjData>();
    }

    public bool HasObj(int uid)
    {
        return BaseObjDic.ContainsKey(uid);
    }
    public void UpdateRefData()
    {
        foreach (var item in BaseObjDic)
            item.Value.UpdateRefData();
    }
}

[System.Serializable]
public class BaseObjData
{
    public bool IsEnemy;
    public int UID;
    public int TID;
    public int X;
    public int Y;
    public GameType.Direction Direction;
    public DataManager.ObjTable RefObjData;
    public void UpdateRefData()
    {
        RefObjData = DataManager.Instance.GetObjTableData(TID);
    }

    public static BaseObjData Create(int uid, int tid, int x, int y, bool isEnemy)
    {
        BaseObjData data = new BaseObjData();
        data.UID = uid;
        data.TID = tid;
        data.X = x;
        data.Y = y;
        data.IsEnemy = isEnemy;
        data.Direction = GameType.Direction.BOTTOM_RIGHT;
        data.UpdateRefData();
        return data;
    }
}


public class CharacterData
{
    public int uid;
    public int tid;
    public bool IsEnemy;
    public int hp;
    public DataManager.Unitinfo refData;
    public DataManager.UnitGradeInfo refUnitGradeData;

    public static CharacterData Create(int _uid, int _tid, int _grade, bool _isEnemy)
    {
        CharacterData data = new CharacterData() {
            uid = _uid,
            tid = _tid,
            IsEnemy = _isEnemy,
            refData = DataManager.Instance.GetUnitinfoData(_tid),
            refUnitGradeData = DataManager.Instance.GetUnitGrade(_tid, _grade)
        };
        data.hp = data.refUnitGradeData.hp;

        return data;
    }
}
