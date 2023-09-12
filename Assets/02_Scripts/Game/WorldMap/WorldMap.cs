using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldMap : MonoBehaviour
{

    private List<WorldMapTileObj> tileList;
    private Dictionary<int, WorldMapStageSlot> stageDic;
    // Start is called before the first frame update
    void Start()
    {
        tileList = new List<WorldMapTileObj>();
        GetComponentsInChildren(tileList);
        List<WorldMapStageSlot> stageSlotsList = new List<WorldMapStageSlot>();
        GetComponentsInChildren(stageSlotsList);
        stageDic = stageSlotsList.ToDictionary(item => item.stage);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
