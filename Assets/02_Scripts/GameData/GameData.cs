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
    public int ShipRewardID;
    public long ShipRewardableTime;
    public long FreeGachaRewardableTime;
    public long SignUpTime;
    public long LastPushRewardedTime;
    public long LastLoginTime;
    public int CurrTutorialID;
    public int TutorialSpawnedUnitUID;
    public long StaminaLastSpawnTime;
    public int AttendanceDay;
    public SerializableDictionary<int, int> AttendanceRewardedDic;
    public long NextAttendanceTime;

    public ReactiveProperty<int> Level;
    public ReactiveProperty<long> Soul;
    public ReactiveProperty<long> Stamina;
    public ReactiveProperty<long> Exp;
    public ReactiveProperty<long> Gold;
    public ReactiveProperty<int> UnitSlotCount;
    public SerializableDictionary<int, StageData> StageClearDic;
    public SerializableDictionary<int, UnitData> HeroDataDic;
    public SerializableDictionary<int, int> BattlePartyDic;

    public LocalSaveData()
    {
        uidSeed = 1000;
        CurrTutorialID = 1;
        TutorialSpawnedUnitUID = -1;
        LastPushRewardedTime = -1;
        ShipRewardID = 0;
        AttendanceDay = 1;
        AttendanceRewardedDic = new SerializableDictionary<int, int>();
        StaminaLastSpawnTime = GameTime.Get();
        NextAttendanceTime = GameTime.GetLocalMidnight();
        StageClearDic = new SerializableDictionary<int, StageData>();
        Soul = new ReactiveProperty<long>(0);
        Stamina = new ReactiveProperty<long>(ConfigTable.Instance.StaminaDefaultCount);
        Exp = new ReactiveProperty<long>(0);
        Gold = new ReactiveProperty<long>(ConfigTable.Instance.GoldDefault);
        HeroDataDic = new SerializableDictionary<int, UnitData>();
        BattlePartyDic = new SerializableDictionary<int, int>();
        Level = new ReactiveProperty<int>(1);

        Enumerable.Range(0, Game.GameConfig.MaxBattlePartyCount).ToList().ForEach(i =>
        {
            BattlePartyDic[i] = -1;
        });
    }

    public void UpdateRefData()
    {
        foreach (var item in HeroDataDic)
            item.Value.UpdateRefData();

        foreach (var item in StageClearDic)
            item.Value.UpdateRefData();
    }
}

public class UnitBattleData : UnitData
{
    public int killCount;
    public bool isDead;
    public int battleUID;
    public int attackDamage;
    public static UnitBattleData Create(int _battleUID, int _uid, int _tid, int _grade, int _count, bool _isEnemy, int powerRate)
    {
        UnitBattleData data = new UnitBattleData()
        {
            battleUID = _battleUID,
            uid = _uid,
            tid = _tid,
            grade = _grade,
            IsEnemy = _isEnemy,
            count = _count,
            killCount = 0,
            isDead = false
    };
        data.UpdateRefData();
        data.hp = (int)(data.refUnitGradeData.hp * powerRate * 0.01f);
        data.attackDamage = (int)(data.refUnitGradeData.attackdmg * powerRate * 0.01f);
        return data;
    }
}
[System.Serializable]
public class StageData
{
    public static StageData Create(int _stageID)
    {
        StageData data = new StageData()
        {
            stageID = _stageID,
            goldharvestTime = new ReactiveProperty<long>()
        };
        data.UpdateRefData();
        data.GenerateharvestTime();
        return data;
    }

    public DataManager.StageInfo refData;
    public int stageID;
    public bool clear;
    public ReactiveProperty<long> goldharvestTime;

    public void UpdateRefData()
    {
        refData = DataManager.Instance.GetStageInfoData(stageID);
    }

    public void GenerateharvestTime()
    {
        goldharvestTime.Value = GameTime.Get() + refData.goldproductterm;
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
    public int grade;
    public bool attackToEnemy;

    public AttackData(int _uid, int _tid, int _damage, int _grade, bool _attackToEnemy)
    {
        attackerUID = _uid;
        attackerTID = _tid;
        damage = _damage;
        grade = _grade;
        attackToEnemy = _attackToEnemy;
    }
}

public class RewardData
{
    public ITEM_TYPE rewardtype;
    public int rewardid;
    public int rewardcount;
}

