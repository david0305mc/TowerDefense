using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameResultPopup : PopupBase
{
    [SerializeField] private Color enableNextStageColor;
    [SerializeField] private Color disableNextStageColor;
    [SerializeField] private Button homeBtn;
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button nextStageBtn;
    [SerializeField] private GameObject winObject;
    [SerializeField] private GameObject killViewObject;
    [SerializeField] private GameObject loseObject;
    [SerializeField] private List<UICell_UnitKill> uiUnitKillLists;
    [SerializeField] private UIGridView gridView = default;
    protected override void Awake()
    {
        base.Awake();
    }


    private void InitBattleParty()
    {
        Enumerable.Range(0, uiUnitKillLists.Count).ToList().ForEach(i =>
        {
            int unitUID = UserData.Instance.GetBattlePartyUIDByIndex(i);
            if (unitUID != -1)
            {
                uiUnitKillLists[i].SetActive(true);
                uiUnitKillLists[i].SetData(i, unitUID, (_slotIndex) =>
                {

                });
            }
            else
            {
                uiUnitKillLists[i].SetActive(false);
            }
            
        });
    }

    public void SetData(bool isWin, List<DataManager.StageRewardInfo> rewards, System.Action _homeAction, System.Action _retryAction, System.Action _nextStageAction)
    {
        InitBattleParty();
        
        List<UIRewardCellData> rewardLists = new List<UIRewardCellData>();
        rewardLists.Add(new UIRewardCellData(0));
        foreach (var rewardInfo in rewards)
        {
            rewardLists.Add(new UIRewardCellData(rewardInfo.id));
        }
        gridView.UpdateContents(rewardLists.ToArray());
        gridView.OnCellClicked(index =>
        {
            //SelectCell(index);
        });

        killViewObject.SetActive(true);
        if (isWin)
        {
            winObject.SetActive(true);
            loseObject.SetActive(false); 
        }
        else
        {
            winObject.SetActive(false);
            loseObject.SetActive(true);
        }

        homeBtn.onClick.RemoveAllListeners();
        homeBtn.onClick.AddListener(() => {
            Hide();
            _homeAction?.Invoke();
        });

        retryBtn.onClick.RemoveAllListeners();
        retryBtn.onClick.AddListener(() => {
            Hide();
            _retryAction?.Invoke();
        });

        nextStageBtn.onClick.RemoveAllListeners();
        int nextStage = UserData.Instance.PlayingStage + 1;
        if (DataManager.Instance.GetStageInfoData(nextStage) == null)
        {
            // there is no nextStage
            nextStageBtn.image.color = disableNextStageColor;
            nextStageBtn.onClick.AddListener(() => {
                PopupManager.Instance.ShowSystemOneBtnPopup("There is no Stage", "OK");
            });
        }
        else
        {
            var nextStageStatus = UserData.Instance.GetStageStatus(nextStage);
            if (nextStageStatus == Game.StageStatus.Lock)
            {
                nextStageBtn.image.color = disableNextStageColor;
                nextStageBtn.onClick.AddListener(() => {
                    PopupManager.Instance.ShowSystemOneBtnPopup("Lock Stage", "OK");
                });
            }
            else
            {
                nextStageBtn.image.color = enableNextStageColor;
                nextStageBtn.onClick.AddListener(() => {
                    Hide();
                    _nextStageAction?.Invoke();
                });
            }
        }

        if (UserData.Instance.LocalData.CurrTutorialID == 9)
        {
            MGameManager.Instance.PlayNextTutorial(()=> {
                Hide();
                _homeAction?.Invoke();
            });
        }
    }
}
