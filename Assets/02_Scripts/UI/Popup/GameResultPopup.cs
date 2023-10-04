using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameResultPopup : PopupBase
{
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

        if (isWin)
        {
            killViewObject.SetActive(true);
            winObject.SetActive(true);
            loseObject.SetActive(false);

            List<UIRewardCellData> rewardLists = new List<UIRewardCellData>();
            rewardLists.Add(new UIRewardCellData(0));

            foreach (var rewardInfo in rewards)
            {
                rewardLists.Add(new UIRewardCellData(rewardInfo.id));
            }

            //var itemData = Enumerable.Range(0, 10).Select(i => new UIRewardCellData(i)).ToArray();
            gridView.UpdateContents(rewardLists.ToArray());

            gridView.OnCellClicked(index =>
            {
                //SelectCell(index);
            });

        }
        else
        {
            winObject.SetActive(false);
            killViewObject.SetActive(false);
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
        nextStageBtn.onClick.AddListener(() => {
            Hide();
            _nextStageAction?.Invoke();
        });
    }
}
