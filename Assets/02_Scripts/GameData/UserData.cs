using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UniRx;
using System.Linq;

public partial class UserData : Singleton<UserData>
{

    private static readonly string LocalFilePath = Path.Combine(Application.persistentDataPath, "LocalData");
    private static readonly string InBuildDataImportPath = "InBuildData";
    private static readonly string InBuildDataExportPath = Path.Combine(Application.persistentDataPath, "InBuildData");

    public int PlayingStage { get; set; }
    public int BattlePower { get; private set; }
    public int GameSpeed { get; private set; }

    public LocalSaveData LocalData { get; set; }
    public Dictionary<int, UnitBattleData> enemyDataDic;
    public Dictionary<int, UnitBattleData> battleHeroDataDic;

    public ReactiveProperty<int> AcquireGold;

    public bool isAllHeroDead()
    {
        var aliveHeroes = battleHeroDataDic.Where(i => i.Key != -1 && !i.Value.isDead);
        return aliveHeroes.Count() == 0;
    }
    public List<UnitBattleData> GetAliveEnemyLists()
    {
        return enemyDataDic.Values.Where(enemyUnit =>
        {
            if (enemyUnit.battleUID == -1)
                return false;
            if (enemyUnit.isDead)
                return false;
            if (enemyUnit.tid < Game.GameConfig.StartBuildingID) // Except BuildingUnit
                return false;
            if (enemyUnit.uid == MGameManager.Instance.EnemyBossUID)
                return true;
            return false;
        }).ToList();
    }
    
    public bool IsClearedStage(int _stage) => LocalData.StageClearDic.ContainsKey(_stage);

    public ReactiveProperty<bool> IsEnemyItemSelected { get; set; }

    public int ShopSelectedItem { get; set; }
    public int GetPartySlotIndexByUID(int _uid)
    {
        KeyValuePair<int, int> data = LocalData.BattlePartyDic.FirstOrDefault(i => i.Value == _uid);
        if (data.Equals(default(KeyValuePair<int, int>)))
        {
            return -1;
        }
        return data.Key;
    }
    public int GetBattlePartyUIDByIndex(int _index)
    {
        return LocalData.BattlePartyDic[_index];
    }
    public void InitData()
    {
        GameSpeed = 1;
        ShopSelectedItem = -1;
        enemyDataDic = new Dictionary<int, UnitBattleData>();
        battleHeroDataDic = new Dictionary<int, UnitBattleData>();
        IsEnemyItemSelected = new ReactiveProperty<bool>(false);
        AcquireGold = new ReactiveProperty<int>(0);
    }

    public void UpdateData()
    {
        CalcBattlePower();
    }

    public int NextGameSpeed() 
    {
        if (GameSpeed == 1)
        {
            GameSpeed = 2;
        }
        else if (GameSpeed == 2)
        {
            GameSpeed = 4;
        }
        else 
        {
            GameSpeed = 1;
        }
        return GameSpeed;
    }

    private void InitNewGameData()
    {

    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < Game.GameConfig.MaxBattlePartyCount; i++)
        {
            if (LocalData.BattlePartyDic[i] == -1)
            {
                return i;
            }
        }
        return -1;
    }
    public int AddBattleParty(int _heroUID)
    {
        int emptySlotIndex = FindEmptySlot();
        LocalData.BattlePartyDic[emptySlotIndex] = _heroUID;
        CalcBattlePower();
        return emptySlotIndex;
    }

    public void RemoveBattleParty(int _slotIndex)
    {
        LocalData.BattlePartyDic[_slotIndex] = -1;
        CalcBattlePower();
    }

    private void CalcBattlePower()
    {
        BattlePower = 0;
        foreach (var item in LocalData.BattlePartyDic)
        {
            if (item.Value != -1)
            {
                var heroData = GetHeroData(item.Value);
                BattlePower += heroData.refUnitGradeData.combatpower;
            }
        }
    }

    public void ClearStage(int _stage)
    {
        LocalData.StageClearDic[_stage] = 1;
    }

    public int GetLatestStage()
    {
         return LocalData.StageClearDic.Max(i => i.Key);
    }

    public Game.StageStatus GetStageStatus(int _stageID)
    {
        var stageInfo = DataManager.Instance.GetStageInfoData(_stageID);
        if (LocalData.StageClearDic.ContainsKey(stageInfo.priorstageid))
        {
            if (LocalData.StageClearDic.ContainsKey(_stageID))
            {
                return Game.StageStatus.Occupation;
            }
            else
            {
                return Game.StageStatus.Normal;
            }
        }
        else
        {
            return Game.StageStatus.Lock;
        }
    }

    public bool isUnitDead(int _uid, bool isEnemy)
    {
        if (isEnemy)
            return IsEnemyDead(_uid);
        return isBattleHeroDead(_uid);
    }
    public UnitData GetUnitData(int _uid, bool isEnemy)
    {
        if (isEnemy)
            return GetEnemyData(_uid);
        return GetBattleHeroData(_uid);
    }

    public bool IsEnemyDead(int _uid)
    {
        return enemyDataDic[_uid].isDead;
    }

    public UnitBattleData GetEnemyData(int _uid)
    {
        if (enemyDataDic.ContainsKey(_uid))
            return enemyDataDic[_uid];
        return null;
    }

    public UnitData GetHeroData(int _uid)
    {
        if (LocalData.HeroDataDic.ContainsKey(_uid))
            return LocalData.HeroDataDic[_uid];
        return null;
    }

    public bool isBattleHeroDead(int _battleUID)
    {
        try
        {
            return battleHeroDataDic[_battleUID].isDead;
        }
        catch
        {
            Debug.LogError($"Error {_battleUID}");
        }
        return false;
    }

    public int GetBattleKillCount(int _uid)
    {
        return battleHeroDataDic.Where(item => item.Value.uid == _uid).Sum(item2 => item2.Value.killCount);
    }
    public UnitBattleData GetBattleHeroData(int _battleUID)
    {
        if (battleHeroDataDic.ContainsKey(_battleUID))
            return battleHeroDataDic[_battleUID];
        return null;
    }
    public InBuildData LoadInBuildData()
    {
        var textAsset = Resources.Load("textData") as TextAsset;

        if (textAsset != null)
        {
            return JsonUtility.FromJson<InBuildData>(textAsset.ToString());
        }
        else
        {
            Debug.LogError("Load EnemyData Error");
            return null;
        }
    }

    public void SaveEnemyData()
    {
        //InBuildData EnemyData = new InBuildData();
        //foreach (var item in LocalData.BaseObjDic)
        //{
        //    if (item.Value.IsEnemy)
        //    {
        //        EnemyData.BaseObjDic.Add(item.Key, item.Value);

        //    }
        //}

        //var saveData = JsonUtility.ToJson(EnemyData);
        ////saveData = Utill.EncryptXOR(saveData);
        //Utill.SaveFile(InBuildDataExportPath, saveData);
    }

    public void LoadLocalData()
    {
        if (File.Exists(LocalFilePath))
        {
            var localData = Utill.LoadFromFile(LocalFilePath);
            //localData = Utill.EncryptXOR(localData);
            LocalData = JsonUtility.FromJson<LocalSaveData>(localData);
            LocalData.UpdateRefData();
        }
        else
        {
            // NewGame
            LocalData = new LocalSaveData();
            InitNewGameData();
        }
    }

    public void SaveLocalData()
    {
        var saveData = JsonUtility.ToJson(LocalData);
        //saveData = Utill.EncryptXOR(saveData);
        Utill.SaveFile(LocalFilePath, saveData);
    }

    /// New Game

    public UnitBattleData AddEnemyData(int _tid)
    {
        var data = UnitBattleData.Create(MGameManager.GenerateFlashUID(), -1, _tid, 1, 1, true);
        enemyDataDic.Add(data.battleUID, data);
        return data;
    }

    public bool AttackToEnmey(int _enemyUID, int _damage)
    {
        if (enemyDataDic[_enemyUID].isDead)
        {
            Debug.LogError($"already detroyed enemy {_enemyUID}");
            return false;
        }
        var enemyData = enemyDataDic[_enemyUID];
        enemyData.hp -= _damage;
        if (enemyData.hp <= 0)
        {
            return true;
        }
        return false;
    }
    public bool AttackToHero(int _heroUID, int _damage)
    {
        if (battleHeroDataDic[_heroUID].isDead)
        {
            Debug.LogError($"already detroyed hero {_heroUID}");
            return false;
        }
        var heroData = battleHeroDataDic[_heroUID];
        heroData.hp -= _damage;
        if (heroData.hp <= 0)
        {
            return true;
        }
        return false;
    }

    public void RemoveEnmey(int _enemyUID)
    {
        enemyDataDic.Remove(_enemyUID);
    }

    public UnitBattleData AddBattleHeroData(UnitData _heroData)
    {
        var data = UnitBattleData.Create(MGameManager.GenerateFlashUID(), _heroData.uid, _heroData.tid, _heroData.refUnitGradeData.grade, _heroData.count, false);
        battleHeroDataDic.Add(data.battleUID, data);
        return data;
    }

    public void KillEnemy(int _heroUID, int _enemyUID)
    {
        enemyDataDic[_enemyUID].isDead = true;
        battleHeroDataDic[_heroUID].killCount++;
    }

    public void KillBattleHero(int _enemyUID, int _heroUID)
    {
        battleHeroDataDic[_heroUID].isDead = true;
        enemyDataDic[_enemyUID].killCount++;

    }
    public void RemoveBattleHero(int _heroUID)
    {
        battleHeroDataDic.Remove(_heroUID);
    }

    public void UpgradeHero(int _heroUID)
    {
        var heroData = GetHeroData(_heroUID);
        heroData.grade++;
        heroData.count -= heroData.refUnitGradeData.upgradepiececnt;
        LocalData.Soul.Value -= heroData.refUnitGradeData.upgradecostcnt;
    }

}
