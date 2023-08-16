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
    public Dictionary<int, CharacterData> enemyDataDic;

    public  ReactiveProperty<bool> IsEnemyItemSelected { get; set; }

    public int ShopSelectedItem { get; set; } 
    public void InitData()
    {
        ShopSelectedItem = -1;
        enemyDataDic = new Dictionary<int, CharacterData>();
        IsEnemyItemSelected = new ReactiveProperty<bool>(false);
    }

    public void RemoveObj(int uid)
    {
        LocalData.BaseObjDic.Remove(uid);
    }

    public CharacterData GetEnemyData(int _uid)
    {
        if(enemyDataDic.ContainsKey(_uid))
            return enemyDataDic[_uid];
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
        InBuildData EnemyData = new InBuildData();
        foreach (var item in LocalData.BaseObjDic)
        {
            if (item.Value.IsEnemy)
            {
                EnemyData.BaseObjDic.Add(item.Key, item.Value);
                
            }
        }
        
        var saveData = JsonUtility.ToJson(EnemyData);
        //saveData = Utill.EncryptXOR(saveData);
        Utill.SaveFile(InBuildDataExportPath, saveData);
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

    public BaseObjData CreateBaseObjData(int tid, int x, int y, bool isEnemy)
    {
        BaseObjData data = BaseObjData.Create(GameManager.GenerateUID(), tid, x, y, isEnemy);
        LocalData.BaseObjDic.Add(data.UID, data);
        return data;
    }



    /// New Game
    
    public CharacterData AddEnemyData(int _tid)
    {
        var data = CharacterData.Create(MGameManager.GenerateUID(), _tid, true);
        enemyDataDic.Add(data.uid, data);
        return data;
    }

    public bool AttackToEnmey(int _enemyUID, int _damage)
    {
        var enemyData = enemyDataDic[_enemyUID];
        enemyData.hp -= _damage;
        if (enemyData.hp <= 0)
        {
            RemoveEnmey(_enemyUID);
            return true;
        }
        return false;
    }

    public void RemoveEnmey(int _enemyUID)
    {
        enemyDataDic.Remove(_enemyUID);
    }
}
