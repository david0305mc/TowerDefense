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
    public LocalData LocalData { get; set; }

    public  ReactiveProperty<bool> IsEnemyItemSelected { get; set; }

    public int ShopSelectedItem { get; set; } 
    public void InitData()
    {
        ShopSelectedItem = -1;
        IsEnemyItemSelected = new ReactiveProperty<bool>(false);
    }

    public void RemoveObj(int uid)
    {
        LocalData.BaseObjDic.Remove(uid);
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
}
