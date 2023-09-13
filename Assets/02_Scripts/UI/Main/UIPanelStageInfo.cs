using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPanelStageInfo : MonoBehaviour
{
    [SerializeField] private Button startBtn;
    [SerializeField] private TextMeshProUGUI titleText;
    private System.Action startBtnAction;
    private void Awake()
    {
        startBtn.onClick.AddListener(() => {
            startBtnAction?.Invoke();
        });
    }

    public void SetData(int _stageID, System.Action _action)
    {
        startBtnAction = _action;
        titleText.SetText(_stageID.ToString());
        var stageData = UserData.Instance.GetStageData(_stageID);
        switch (stageData.status)
        {
            case Game.StageStatus.Lock:
                startBtn.SetActive(false);
                break;
            case Game.StageStatus.Normal:
                startBtn.SetActive(true);
                break;
            case Game.StageStatus.Occupation:
                startBtn.SetActive(true);
                break;
        }
    }
}
