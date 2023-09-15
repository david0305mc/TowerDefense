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

    public LocalData()
    {
        uidSeed = 0;
        Gold = new ReactiveProperty<long>(0);
    }

    public void UpdateRefData()
    {
        //foreach (var item in BaseObjDic)
        //    item.Value.UpdateRefData();
    }
}

public class UnitData
{
    public int uid;
    public int tid;
    public bool IsEnemy;
    public int hp;
    public DataManager.Unitinfo refData;
    public DataManager.UnitGradeInfo refUnitGradeData;

    public static UnitData Create(int _uid, int _tid, int _grade, bool _isEnemy)
    {
        UnitData data = new UnitData() {
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

public class AttackData
{
    public int attackerUID;
    public int attackerTID;
    public int damage;
    public bool attackToEnemy;

    public AttackData(int _uid, int _tid, int _damage, bool _attackToEnemy)
    {
        attackerUID = _uid;
        attackerTID = _tid;
        damage = _damage;
        attackToEnemy = _attackToEnemy;
    }
}

public class StageData
{
    public int stageID;
    public StageStatus status;
    public DataManager.StageInfo refData;

    public static StageData Create(int _stageID, StageStatus _status)
    {
        StageData data = new StageData()
        {
            stageID = _stageID, status = _status,
            refData = DataManager.Instance.GetStageInfoData(_stageID)
        };
        return data;
    }
}
