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
    public LocalData LocalData { get; set; }
    public Dictionary<int, UnitData> enemyDataDic;
    public Dictionary<int, UnitData> heroDataDic;
    public Dictionary<int, UnitData> battleHeroDataDic;
    private Dictionary<int, int> battlePartyDic;
    public Dictionary<int, int> BattlePartyDic => battlePartyDic;

    private Dictionary<int, StageData> stageDataDic;
    private HashSet<int> stageClearSet;
    public StageData GetStageData(int _stage) => stageDataDic.GetValueOrDefault(_stage);
    public bool IsClearedStage(int _stage) => stageClearSet.Contains(_stage);

    public  ReactiveProperty<bool> IsEnemyItemSelected { get; set; }

    public int ShopSelectedItem { get; set; }
    public int GetPartySlotIndexByUID(int _uid)
    {
        KeyValuePair<int, int> data = battlePartyDic.FirstOrDefault(i => i.Value == _uid);
        if (data.Equals(default(KeyValuePair<int, int>)))
        {
            return -1;
        }
        return data.Key;
    }
    public int GetPartyUIDByIndex(int _index)
    {
        return battlePartyDic[_index];
    }
    public void InitData()
    {
        ShopSelectedItem = -1;
        enemyDataDic = new Dictionary<int, UnitData>();
        heroDataDic = new Dictionary<int, UnitData>();
        battleHeroDataDic = new Dictionary<int, UnitData>();
        IsEnemyItemSelected = new ReactiveProperty<bool>(false);
        stageDataDic = new Dictionary<int, StageData>();
        stageClearSet = new HashSet<int>();

        battlePartyDic = new Dictionary<int, int>();
        Enumerable.Range(0, Game.GameConfig.MaxBattlePartyCount).ToList().ForEach(i =>
        {
            battlePartyDic[i] = -1;
        });
    }

    private void InitBeginData()
    {
        AddHeroData(2001);
        AddHeroData(2001);
        AddHeroData(2001);
        AddHeroData(2002);
        AddHeroData(2002);
    }

    private void InitStage()
    {
        stageClearSet.Add(0);
        foreach (var item in DataManager.Instance.StageinfoArray)
        {
            var data = StageData.Create(item.id, Game.StageStatus.Lock);
            stageDataDic.Add(data.stageID, data);
        }
        UpdateStageStatus();
    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < Game.GameConfig.MaxBattlePartyCount; i++)
        {
            if (battlePartyDic[i] == -1)
            {
                return i;
            }
        }
        return -1;
    }
    public int AddBattleParty(int _heroUID)
    {
        int emptySlotIndex = FindEmptySlot();
        battlePartyDic[emptySlotIndex] = _heroUID;
        return emptySlotIndex;
    }

    public void RemoveBattleParty(int _slotIndex)
    {
        int heroUid = battlePartyDic[_slotIndex];
        battlePartyDic[_slotIndex] = -1;
    }

    public void ClearStage(int _stage)
    {
        stageClearSet.Add(_stage);
        UpdateStageStatus();
    }

    private void UpdateStageStatus()
    {
        foreach (var item in stageDataDic)
        {
            if (stageClearSet.Contains(item.Value.refData.priorstageid))
            {
                if (stageClearSet.Contains(item.Key))
                {
                    item.Value.status = Game.StageStatus.Occupation;
                }
                else
                {
                    item.Value.status = Game.StageStatus.Normal;
                }
            }
            else
            {
                item.Value.status = Game.StageStatus.Lock;
            }
        } 
    }

    public UnitData GetUnitData(int _uid, bool isEnemy)
    {
        if (isEnemy)
            return GetEnemyData(_uid);
        return GetBattleHeroData(_uid);
    }
    public UnitData GetEnemyData(int _uid)
    {
        if(enemyDataDic.ContainsKey(_uid))
            return enemyDataDic[_uid];
        return null;
    }
    public UnitData GetHeroData(int _uid)
    {
        if (heroDataDic.ContainsKey(_uid))
            return heroDataDic[_uid];
        return null;
    }
    public UnitData GetBattleHeroData(int _uid)
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
            LocalData = JsonUtility.FromJson<LocalData>(localData);
            LocalData.UpdateRefData();
            InitStage();
            InitBeginData();
        }
        else
        {
            // NewGame
            LocalData = new LocalData();
            InitStage();
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
        var data = UnitData.Create(MGameManager.GenerateUID(), _tid, 1, true);
        enemyDataDic.Add(data.uid, data);
        return data;
    }

    public bool AttackToEnmey(int _enemyUID, int _damage)
    {
        if (!enemyDataDic.ContainsKey(_enemyUID))
        {
            Debug.LogError($"already detroyed {_enemyUID}");
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
        if (!battleHeroDataDic.ContainsKey(_heroUID))
        {
            Debug.LogError($"already detroyed {_heroUID}");
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

    public UnitData AddHeroData(int _tid)
    {
        var data = UnitData.Create(MGameManager.GenerateUID(), _tid, 1, false);
        heroDataDic.Add(data.uid, data);
        return data;
    }

    public void RemoveHero(int _heroUID)
    {
        heroDataDic.Remove(_heroUID);
    }

    public UnitData AddBattleHeroData(UnitData _heroData)
    {
        var data = UnitData.Create(MGameManager.GenerateUID(), _heroData.tid, _heroData.refUnitGradeData.grade, false);
        battleHeroDataDic.Add(data.uid, data);
        return data;
    }

    public void RemoveBattleHero(int _heroUID)
    {
        battleHeroDataDic.Remove(_heroUID);
    }

}
