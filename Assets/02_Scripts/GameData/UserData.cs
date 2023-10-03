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

    public int CurrStage { get; set; }

    public LocalSaveData LocalData { get; set; }
    public Dictionary<int, UnitBattleData> enemyDataDic;
    public Dictionary<int, UnitBattleData> battleHeroDataDic;

    public ReactiveProperty<int> AcquireGold;

    public bool isEmptyBattleHero() => battleHeroDataDic.Where(i=>i.Value.isDead).Count() == 0;
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
        ShopSelectedItem = -1;
        enemyDataDic = new Dictionary<int, UnitBattleData>();
        battleHeroDataDic = new Dictionary<int, UnitBattleData>();
        IsEnemyItemSelected = new ReactiveProperty<bool>(false);
        AcquireGold = new ReactiveProperty<int>(0);
    }

    private void InitBeginData()
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
        return emptySlotIndex;
    }

    public void RemoveBattleParty(int _slotIndex)
    {
        LocalData.BattlePartyDic[_slotIndex] = -1;
    }

    public void ClearStage(int _stage)
    {
        LocalData.StageClearDic[_stage] = 1;
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

    public UnitData GetEnemyData(int _uid)
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

    public bool isBattleHeroDead(int _uid)
    {
        return battleHeroDataDic[_uid].isDead;
    }

    public UnitBattleData GetBattleHeroData(int _uid)
    {
        if (battleHeroDataDic.ContainsKey(_uid))
            return battleHeroDataDic[_uid];
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
            InitBeginData();
        }
        else
        {
            // NewGame
            LocalData = new LocalSaveData();
            InitBeginData();
        }
    }

    public void SaveLocalData()
    {
        var saveData = JsonUtility.ToJson(LocalData);
        //saveData = Utill.EncryptXOR(saveData);
        Utill.SaveFile(LocalFilePath, saveData);
    }

    /// New Game

    public UnitData AddEnemyData(int _tid)
    {
        var data = UnitBattleData.Create(MGameManager.GenerateUID(), _tid, 1, 1, true);
        enemyDataDic.Add(data.uid, data);
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
        var data = UnitBattleData.Create(_heroData.uid, _heroData.tid, _heroData.refUnitGradeData.grade, _heroData.count, false);
        battleHeroDataDic.Add(data.uid, data);
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
