using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UniRx;

public partial class UserData : Singleton<UserData>
{

    private static readonly string LocalFilePath = Path.Combine(Application.persistentDataPath, "LocalData");
    private static readonly string InBuildDataImportPath = "InBuildData";
    private static readonly string InBuildDataExportPath = Path.Combine(Application.persistentDataPath, "InBuildData");

    public int CurrStage { get; set; }
    public LocalData LocalData { get; set; }
    public Dictionary<int, UnitData> enemyDataDic;
    public Dictionary<int, UnitData> heroDataDic;

    public  ReactiveProperty<bool> IsEnemyItemSelected { get; set; }

    public int ShopSelectedItem { get; set; } 
    public void InitData()
    {
        ShopSelectedItem = -1;
        enemyDataDic = new Dictionary<int, UnitData>();
        heroDataDic = new Dictionary<int, UnitData>();
        IsEnemyItemSelected = new ReactiveProperty<bool>(false);
    }

    public UnitData GetUnitData(int _uid, bool isEnemy)
    {
        if (isEnemy)
            return GetEnemyData(_uid);
        return GetHeroData(_uid);
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
        }
        else
        {
            // NewGame
            LocalData = new LocalData();
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
        if (!heroDataDic.ContainsKey(_heroUID))
        {
            Debug.LogError($"already detroyed {_heroUID}");
            return false;
        }
        var heroData = heroDataDic[_heroUID];
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

}
