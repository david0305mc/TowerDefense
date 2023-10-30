using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldMap : MonoBehaviour
{
    private List<WorldMapTileObj> tileLists;
    private List<WorldMapStageObj> stageObjLists;
    private Dictionary<int, WorldMapStageSlot> stageDic;

    private void Awake()
    {
        tileLists = new List<WorldMapTileObj>();
        stageObjLists = new List<WorldMapStageObj>();
        GetComponentsInChildren(tileLists);
        GetComponentsInChildren(true, stageObjLists);
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
        foreach (var item in tileLists)
        {
            item.UpdateData();
        }

        foreach (var item in stageDic)
        {
            item.Value.UpdateData();
        }

        foreach (var item in stageObjLists)
        {
            item.UpdateData();
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
            var nextStageStatus = UserData.Instance.GetStageStatus(latestStageID + 1);
            if (nextStageStatus == Game.StageStatus.Lock)
            {
                return stageDic[latestStageID].gameObject;
            }
            else
            {
                return stageDic[latestStageID + 1].gameObject;
            }
        }
    }

    public WorldMapStageSlot GetStageSlotObj(int _stageID)
    {
        return stageDic[_stageID];
    }
}
