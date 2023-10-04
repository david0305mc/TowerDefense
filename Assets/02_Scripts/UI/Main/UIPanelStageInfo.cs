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
    [SerializeField] private TextMeshProUGUI battlePowerText;
    [SerializeField] private TextMeshProUGUI requrePowerText;
    [SerializeField] private TextMeshProUGUI requreEnegyText;
    [SerializeField] private UIGridView gridView = default;
    [SerializeField] private Color disableColor;
    [SerializeField] private Color enableColor;

    private System.Action startBtnAction;
    private System.Action closeBtnAction;
    private void Awake()
    {
        startBtn.onClick.AddListener(() => {
            startBtnAction?.Invoke();
            //if (UserData.Instance.LocalData.Stamina.Value >= ConfigTable.Instance.StageStartCost)
            //{
            //    startBtnAction?.Invoke();
            //}
            //else
            //{
            //    PopupManager.Instance.ShowSystemOneBtnPopup("Not Enough Stamina", "OK");
            //}
        });
        closeBtn.onClick.AddListener(() =>
        {
            closeBtnAction?.Invoke();
        });
    }

    public void SetData(int _stageID, System.Action _startAction, System.Action _closeAction)
    {
        var stageInfo = DataManager.Instance.GetStageInfoData(_stageID);
        startBtnAction = _startAction;
        closeBtnAction = _closeAction;
        titleText.SetText(LocalizeManager.Instance.GetLocalString(stageInfo.stagename));
        battlePowerText.SetText(UserData.Instance.BattlePower.ToString());
        requrePowerText.SetText(stageInfo.needcombatpower.ToString());
        requreEnegyText.SetText(ConfigTable.Instance.StageStartCost.ToString());

        if (UserData.Instance.LocalData.Stamina.Value >= ConfigTable.Instance.StageStartCost)
        {
            requreEnegyText.color = enableColor;
        }
        else
        {
            requreEnegyText.color = disableColor;
        }
        
        var stageStatus = UserData.Instance.GetStageStatus(_stageID);
        switch (stageStatus)
        {
            case Game.StageStatus.Lock:
                startBtn.SetActive(false);
                requrePowerText.color = disableColor;
                break;
            case Game.StageStatus.Normal:
                startBtn.SetActive(true);
                requrePowerText.color = enableColor;
                break;
            case Game.StageStatus.Occupation:
                startBtn.SetActive(true);
                requrePowerText.color = enableColor;
                break;
        }

        List<UIRewardCellData> rewardLists = new List<UIRewardCellData>();
        List<DataManager.StageRewardInfo> rewards = DataManager.Instance.GetStageRewards(_stageID);
        foreach (var rewardInfo in rewards)
        {
            rewardLists.Add(new UIRewardCellData(rewardInfo.id));
        }
        gridView.UpdateContents(rewardLists.ToArray());
        gridView.OnCellClicked(index =>
        {
        });
    }
}
