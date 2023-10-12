using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldMap : MonoBehaviour
{
    private List<WorldMapTileObj> tileList;
    private Dictionary<int, WorldMapStageSlot> stageDic;

    private void Awake()
    {
        tileList = new List<WorldMapTileObj>();
        GetComponentsInChildren(tileList);
        List<WorldMapStageSlot> stageSlotsList = new List<WorldMapStageSlot>();
        GetComponentsInChildren(stageSlotsList);
        stageDic = stageSlotsList.ToDictionary(item => item.stage);
    }

    public void InitWorld()
    {
        UpdateWorld();
    }

    public void UpdateWorld()
    {
        foreach (var item in tileList)
        {
            item.UpdateData();
        }

        foreach (var item in stageDic)
        {
            item.Value.UpdateData();
        }
    }

    public void SelectStage(int _stage)
    {
        foreach (var item in stageDic)
        {
            item.Value.SetSelected(item.Key == _stage);
        }
    }

    public GameObject GetCurrStageObj()
    {
        int latestStageID = UserData.Instance.GetLatestStage();
        var nextStageInfo = DataManager.Instance.GetStageInfoData(latestStageID + 1);
        if (nextStageInfo == null)
        {
            return stageDic[latestStageID].gameObject;
        }
        else
        {
            return stageDic[latestStageID + 1].gameObject;
        }
    }
}
