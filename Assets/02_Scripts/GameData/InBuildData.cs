using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;


[System.Serializable]
public class InBuildData
{
    public SerializableDictionary<int, BaseObjData> BaseObjDic;

    public InBuildData()
    {
        BaseObjDic = new SerializableDictionary<int, BaseObjData>();
    }

    public bool HasObj(int uid)
    {
        return BaseObjDic.ContainsKey(uid);
    }
    public void UpdateRefData()
    {
        foreach (var item in BaseObjDic)
            item.Value.UpdateRefData();
    }

    public void SaveData()
    { 
    
    }

    //public static void SaveLocalData()
    //{
    //    var saveData = JsonUtility.ToJson(LocalData);
    //    //saveData = Utill.EncryptXOR(saveData);
    //    Utill.SaveFile(LocalFilePath, saveData);
    //}


}