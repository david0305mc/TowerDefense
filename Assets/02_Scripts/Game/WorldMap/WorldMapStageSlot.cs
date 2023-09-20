using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapStageSlot : MonoBehaviour
{
    [SerializeField] private GameObject defaultObject;
    [SerializeField] private GameObject lockObject;
    [SerializeField] private GameObject battleObject;

    public int stage;

    public void UpdateData()
    {
        Game.StageStatus stageStatus = UserData.Instance.GetStageStatus(stage);
        switch (stageStatus)
        {
            case Game.StageStatus.Normal:
                {
                    defaultObject.SetActive(true);
                    lockObject.SetActive(false);
                    battleObject.SetActive(true);
                }
                break;
            case Game.StageStatus.Occupation:
                {
                    defaultObject.SetActive(false);
                    lockObject.SetActive(false);
                    battleObject.SetActive(true);
                }
                break;
            case Game.StageStatus.Lock:
                {
                    defaultObject.SetActive(false);
                    lockObject.SetActive(true);
                    battleObject.SetActive(false);
                }
                break;
        }
    }
}
