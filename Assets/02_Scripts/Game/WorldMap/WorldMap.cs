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
}
