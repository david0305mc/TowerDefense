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
        var stageInfo = DataManager.Instance.GetStageInfoData(_stageID);
        startBtnAction = _startAction;
        closeBtnAction = _closeAction;
        titleText.SetText(_stageID.ToString());
        battlePowerText.SetText(UserData.Instance.BattlePower.ToString());
        requrePowerText.SetText(stageInfo.needcombatpower.ToString());

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
