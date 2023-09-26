using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPanelStageInfo : MonoBehaviour
{
    [SerializeField] private Button startBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI titleText;
    private System.Action startBtnAction;
    private System.Action closeBtnAction;
    private void Awake()
    {
        startBtn.onClick.AddListener(() => {
            startBtnAction?.Invoke();
        });
        closeBtn.onClick.AddListener(() =>
        {
            closeBtnAction?.Invoke();
        });
    }

    public void SetData(int _stageID, System.Action _startAction, System.Action _closeAction)
    {
        startBtnAction = _startAction;
        closeBtnAction = _closeAction;
        titleText.SetText(_stageID.ToString());
        var stageStatus = UserData.Instance.GetStageStatus(_stageID);
        switch (stageStatus)
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
