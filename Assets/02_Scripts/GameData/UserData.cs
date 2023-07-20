using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public partial class UserData : Singleton<UserData>
{

    private static readonly string LocalFilePath = Path.Combine(Application.persistentDataPath, "LocalData");
    public LocalData LocalData { get; set; }

    public int ShopSelectedItem { get; set; } 
    public void InitData()
    {
        ShopSelectedItem = -1;
    }

    public void LoadLocalData()
    {
        if (File.Exists(LocalFilePath))
        {
            var localData = Utill.LoadFromFile(LocalFilePath);
            //localData = Utill.EncryptXOR(localData);
            LocalData = JsonUtility.FromJson<LocalData>(localData);
            foreach (var item in LocalData.TestDic)
            {
                Debug.Log($"item key {item.Key},  value {item.Value}");
            }
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

    public BaseObjData CreateBaseObj(int tid, int x, int y)
    {
        var data = BaseObjData.Create(GameManager.GenerateUID(), tid, x, y);
        LocalData.baseObjDic.Add(data.UID, data);
        return data;
    }
}
