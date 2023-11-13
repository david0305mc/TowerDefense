using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UIPanelStageInfo : MonoBehaviour
{
    [SerializeField] private Button startBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI battlePowerText;
    [SerializeField] private TextMeshProUGUI requrePowerText;
    [SerializeField] private TextMeshProUGUI requreEnegyText;
    [SerializeField] private UICell_Reward uiCellRewardPrefab;
    //[SerializeField] private UIGridView gridView = default;
    [SerializeField] private ScrollRect scrollRect = default;
    [SerializeField] private Color disableColor;
    [SerializeField] private Color enableColor;

    private System.Action startBtnAction;
    private System.Action closeBtnAction;
    private DataManager.StageInfo stageInfo;
    private List<UICell_Reward> uiCellRewardList;
    private void Awake()
    {
        uiCellRewardList = new List<UICell_Reward>();
        startBtn.onClick.AddListener(() => {

            if (UserData.Instance.GetBattlePartyCount() == 0)
            {
                PopupManager.Instance.ShowSystemOneBtnPopup(LocalizeManager.Instance.GetLocalString("needUnitSetMessage"), "OK");
                return;
            }

            if (UserData.Instance.LocalData.Stamina.Value >= ConfigTable.Instance.StageStartCost)
            {
                startBtnAction?.Invoke();
            }
            else
            {
                PopupManager.Instance.ShowSystemOneBtnPopup("Not Enough Stamina", "OK");
            }
        });
        closeBtn.onClick.AddListener(() =>
        {
            closeBtnAction?.Invoke();
        });
    }

    public void SetData(int _stageID, System.Action _startAction, System.Action _closeAction)
    {
        stageInfo = DataManager.Instance.GetStageInfoData(_stageID);
        startBtnAction = _startAction;
        closeBtnAction = _closeAction;
        UpdateUI();
    }

    private void UpdateUI()
    {
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

        var stageStatus = UserData.Instance.GetStageStatus(stageInfo.id);
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
        startBtn.SetActive(true);

        if (uiCellRewardList.Count > 0)
        {
            ClearPool();  
        }
        
        List<DataManager.StageRewardInfo> rewards = DataManager.Instance.GetStageRewards(stageInfo.id);
        foreach (var rewardInfo in rewards)
        {
            var uiCell = Lean.Pool.LeanPool.Spawn(uiCellRewardPrefab, scrollRect.content);
            uiCell.SetData(rewardInfo.rewardid, rewardInfo.rewardtype, rewardInfo.rewardcount);
            uiCellRewardList.Add(uiCell);
        }
    }

    private void OnDisable()
    {
        ClearPool();
    }
    private void ClearPool()
    {
        foreach (var item in uiCellRewardList)
        {
            Lean.Pool.LeanPool.Despawn(item);
        }
        uiCellRewardList.Clear();
    }
    private void OnLocalize()
    {
        UpdateUI();
    }
}
