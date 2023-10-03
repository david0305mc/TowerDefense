using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using UniRx;
using System.Linq;

[System.Serializable]
public class LocalSaveData
{
    public int uidSeed;
    public ReactiveProperty<long> Soul;
    public ReactiveProperty<long> Stamina;
    public ReactiveProperty<long> Exp;
    public SerializableDictionary<int, int> StageClearDic;
    public SerializableDictionary<int, UnitData> HeroDataDic;
    public SerializableDictionary<int, int> BattlePartyDic;

    public LocalSaveData()
    {
        uidSeed = 1000;
        StageClearDic = new SerializableDictionary<int, int>() { { 0, 1 } };
        Soul = new ReactiveProperty<long>(0);
        Stamina = new ReactiveProperty<long>(0);
        Exp = new ReactiveProperty<long>(0);
        HeroDataDic = new SerializableDictionary<int, UnitData>();
        BattlePartyDic = new SerializableDictionary<int, int>();
        Enumerable.Range(0, Game.GameConfig.MaxBattlePartyCount).ToList().ForEach(i =>
        {
            BattlePartyDic[i] = -1;
        });
    }

    public void UpdateRefData()
    {
        foreach (var item in HeroDataDic)
            item.Value.UpdateRefData();
    }
}

public class UnitBattleData : UnitData
{
    public int killCount;
    public bool isDead;
    public static new UnitBattleData Create(int _uid, int _tid, int _grade, int _count, bool _isEnemy)
    {
        UnitBattleData data = new UnitBattleData()
        {
            uid = _uid,
            tid = _tid,
            grade = _grade,
            IsEnemy = _isEnemy,
            count = _count,
            killCount = 0,
            isDead = false
    };
        data.UpdateRefData();
        data.hp = data.refUnitGradeData.hp;
        return data;
    }
}

[System.Serializable]
public class UnitData
{
    public int uid;
    public int tid;
    public bool IsEnemy;
    public int hp;
    public int grade;
    public int count;
    public DataManager.Unitinfo refData;
    public DataManager.UnitGradeInfo refUnitGradeData;
    public bool IsMaxGrade => grade >= refData.maxgrade;

    public static UnitData Create(int _uid, int _tid, int _grade, int _count, bool _isEnemy)
    {
        UnitData data = new UnitData() {
            uid = _uid,
            tid = _tid,
            grade = _grade,
            IsEnemy = _isEnemy,
            count = _count,
        };
        data.UpdateRefData();
        data.hp = data.refUnitGradeData.hp;

        return data;
    }
    public void UpdateRefData()
    {
        refData = DataManager.Instance.GetUnitinfoData(tid);
        refUnitGradeData = DataManager.Instance.GetUnitGrade(tid, grade);
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

