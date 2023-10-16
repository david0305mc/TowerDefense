using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldMapStageSlot : MonoBehaviour
{
    [SerializeField] private GameObject defaultObject;
    [SerializeField] private GameObject lockObject;
    [SerializeField] private GameObject battleObject;
    [SerializeField] private GameObject occupationObject;
    [SerializeField] private GameObject selectedMark;

    [SerializeField] private Slider goldProgressBar;
    [SerializeField] private Button harvestButton;
    [SerializeField] private TextMeshProUGUI harvestGoldText;

    public int stage;

    private void Awake()
    {
        selectedMark.SetActive(false);
    }
    public void SetSelected(bool _value)
    {
        selectedMark.SetActive(_value);
    }
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
                    occupationObject.SetActive(false);
                    goldProgressBar.SetActive(false);
                    harvestButton.SetActive(false);
                }
                break;
            case Game.StageStatus.Occupation:
                {
                    defaultObject.SetActive(false);
                    lockObject.SetActive(false);
                    battleObject.SetActive(false);
                    occupationObject.SetActive(true);
                    goldProgressBar.SetActive(true);
                    harvestButton.SetActive(false);
                }
                break;
            case Game.StageStatus.Lock:
                {
                    defaultObject.SetActive(false);
                    lockObject.SetActive(true);
                    battleObject.SetActive(false);
                    occupationObject.SetActive(false);
                    goldProgressBar.SetActive(false);
                    harvestButton.SetActive(false);
                }
                break;
        }
    }
}
