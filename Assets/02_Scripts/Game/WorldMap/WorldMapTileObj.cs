using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapTileObj : MonoBehaviour
{

    [SerializeField] private GameObject defaultObject;
    [SerializeField] private GameObject lockObject;
    [SerializeField] private GameObject occupationObject;

    [SerializeField] private int stageID;
    public int StageID { get => stageID; }

    public void SetData()
    { 
    
    }
}
