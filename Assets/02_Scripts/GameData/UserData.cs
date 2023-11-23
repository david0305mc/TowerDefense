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
    public long OfflineTimeSeconds{ get; set; }

    public bool IsWaveStage => PlayingStage == Game.GameConfig.WaveStageID_01 || PlayingStage == Game.GameConfig.WaveStageID_02;

    public LocalSaveData LocalData { get; set; }
    public Dictionary<int, UnitBattleData> enemyDataDic;
    public Dictionary<int, UnitBattleData> battleHeroDataDic;

    public ReactiveProperty<int> AcquireSoul;

    public bool isAllHeroDead()
    {
        var aliveHeroes = battleHeroDataDic.Where(i => i.Key != -1 && !i.Value.isDead);
        return aliveHeroes.Count() == 0;
    }
    public List<UnitBattleData> GetAliveEnemyLists()
    {
        return enemyDataDic.Values.Where(enemyUnit =>
        {
            if (enemyUnit.battleUID == MGameManager.Instance.EnemyBossUID)
                return true;
            if (enemyUnit.battleUID == -1)
                return false;
            if (enemyUnit.isDead)
                return false;
            if (enemyUnit.tid >= Game.GameConfig.StartBuildingID) // Except BuildingUnit
                return false;
            return true;
        }).ToList();
    }
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
        AcquireSoul = new ReactiveProperty<int>(0);
    }

    public void UpdateData()
    {
        CalcBattlePower();
        if (LocalData.SignUpTime == 0)
        {
            LocalData.SignUpTime = GameTime.Get();
        }
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
        LocalData = new LocalSaveData();
        var heroData = AddHeroData(ConfigTable.Instance.DefaultUnit01, 1);

        AddBattleParty(heroData.uid);
        var levelInfo = DataManager.Instance.GetLevelData(LocalData.Level.Value);
        LocalData.UnitSlotCount = new ReactiveProperty<int>(levelInfo.unlockslot);
        LocalData.ShipRewardableTime = GameTime.Get();
        LocalData.SignUpTime = GameTime.Get();
        OfflineTimeSeconds = -1;
    }

    public bool HasAttendacneReward()
    {
        for (int i = 1; i < LocalData.AttendanceDay + 1; i++)
        {
            if (!LocalData.AttendanceRewardedDic.ContainsKey(i))
            {
                return true;
            }
            if (LocalData.AttendanceRewardedDic[i] == 0)
            {
                return true;
            }
        }
        return false;
    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < GetUnitSlotCount(); i++)
        {
            if (LocalData.BattlePartyDic[i] == -1)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetBattlePartyCount()
    {
        return LocalData.BattlePartyDic.Where(item => item.Value > 0).Count();
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
        if (!LocalData.StageClearDic.ContainsKey(_stage))
        {
            LocalData.StageClearDic[_stage] = StageData.Create(_stage);
        }
    }

    public int GetLatestStage()
    {
        var clearList = LocalData.StageClearDic.Where(data => data.Key != Game.GameConfig.WaveStageID_01
        && data.Key != Game.GameConfig.WaveStageID_02);

        if (clearList.Count() == 0)
        {
            return 1;
        }

        return clearList.Max(m => m.Key);
    }

    public StageData GetStageData(int _stageID)
    {
        if (LocalData.StageClearDic.TryGetValue(_stageID, out StageData _value))
        {
            return _value;
        }
        return null;
    }
    public Game.StageStatus GetStageStatus(int _stageID)
    {
        var stageInfo = DataManager.Instance.GetStageInfoData(_stageID);
        if (stageInfo.priorstageid == 0 || LocalData.StageClearDic.ContainsKey(stageInfo.priorstageid))
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

    public UnitData GetHeroDataByTID(int _tid)
    {
        return LocalData.HeroDataDic.Values.FirstOrDefault(item => item.tid == _tid);
    }

    public bool isBattleHeroDead(int _battleUID)
    {
        try
        {
            if (_battleUID == Game.GameConfig.UserObjectUID)
                return false;

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
    public UnitBattleData GetBattleHeroDataAlive()
    {
        foreach (var item in battleHeroDataDic)
        {
            if (!item.Value.isDead)
            {
                return item.Value;
            }
        }
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

    public int GetUnitSlotCount()
    {
        return DataManager.Instance.GetLevelData(LocalData.Level.Value).unlockslot;
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

    public bool IsOnTutorial()
    {
        return LocalData.CurrTutorialID < Game.GameConfig.LastTutorial;
    }
    public void LoadLocalData()
    {
        int newUser = PlayerPrefs.GetInt("NewUser", 0);
        if (newUser == 1)
        {
            try
            {
                var localData = Utill.LoadFromFile(LocalFilePath);
                localData = Utill.EncryptXOR(localData);
                LocalData = JsonUtility.FromJson<LocalSaveData>(localData);
                OfflineTimeSeconds = GameTime.Get() - LocalData.LastLoginTime;
            }
            catch
            {
                // NewGame
                InitNewGameData();
                return;
            }

            LocalData.UpdateRefData();
            if (IsOnTutorial())
            {
                // Restart 
                InitNewGameData();
            }
        }
        else
        {
            // NewGame
            InitNewGameData();
        }
    }

    public void SaveLocalData()
    {
        LocalData.LastLoginTime = GameTime.Get();
        var saveData = JsonUtility.ToJson(LocalData);
        saveData = Utill.EncryptXOR(saveData);
        Utill.SaveFile(LocalFilePath, saveData);
        if (PlayerPrefs.GetInt("NewUser", 0) == 0)
        {
            PlayerPrefs.SetInt("NewUser", 1);
        }
    }

    /// New Game

    public UnitBattleData AddEnemyData(int _tid, int _powerRate)
    {
        var data = UnitBattleData.Create(MGameManager.GenerateFlashUID(), -1, _tid, 1, 1, true, _powerRate);
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

    public void CheckAttendance()
    {
        int maxDay = DataManager.Instance.AttendanceDic.Values.Max(item => item.day);

        if (GameTime.Get() >= LocalData.NextAttendanceTime)
        {
            LocalData.AttendanceDay ++;
            if (LocalData.AttendanceDay > maxDay)
            {
                LocalData.AttendanceDay = maxDay;
            }
            LocalData.NextAttendanceTime = GameTime.GetLocalMidnight();
            SaveLocalData();
        }
    }
    public UnitBattleData AddDevilCastleData(int _tid)
    {
        var data = UnitBattleData.Create(MGameManager.GenerateFlashUID(), -1, _tid, 1, 1, false, 100);
        battleHeroDataDic.Add(data.battleUID, data);
        return data;
    }

    public UnitBattleData AddBattleHeroData(UnitData _heroData)
    {
        var data = UnitBattleData.Create(MGameManager.GenerateFlashUID(), _heroData.uid, _heroData.tid, _heroData.refUnitGradeData.grade, _heroData.count, false, 100);
        battleHeroDataDic.Add(data.battleUID, data);
        return data;
    }

    public void KillEnemy(int _heroUID, int _enemyUID)
    {
        enemyDataDic[_enemyUID].isDead = true;
        if (_heroUID == Game.GameConfig.UserObjectUID)
        {
            return;
        }
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
        heroData.UpdateRefData();
    }

}
