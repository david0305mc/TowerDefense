using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapStageObj : MonoBehaviour
{
    [SerializeField] private int stageID;
    public int StageID { get => stageID; }


    public void UpdateData()
    {
        var stageStatus = UserData.Instance.GetStageStatus(StageID);
        switch (stageStatus)
        {
            case Game.StageStatus.Normal:
                {
                    gameObject.SetActive(false);
                }
                break;
            case Game.StageStatus.Occupation:
                {
                    gameObject.SetActive(true);
                }
                break;
            case Game.StageStatus.Lock:
                {
                    gameObject.SetActive(false);
                }
                break;
        }
    }
}
