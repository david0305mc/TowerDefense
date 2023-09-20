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

    public void UpdateData()
    {
        var stageStatus = UserData.Instance.GetStageStatus(StageID);
        switch (stageStatus)
        {
            case Game.StageStatus.Normal:
                {
                    defaultObject.SetActive(true);
                    lockObject.SetActive(false);
                    occupationObject.SetActive(false);
                }
                break;
            case Game.StageStatus.Occupation:
                {
                    defaultObject.SetActive(false);
                    lockObject.SetActive(false);
                    occupationObject.SetActive(true);
                }
                break;
            case Game.StageStatus.Lock:
                {
                    defaultObject.SetActive(false);
                    lockObject.SetActive(true);
                    occupationObject.SetActive(false);
                }
                break;
        }
    }
}
