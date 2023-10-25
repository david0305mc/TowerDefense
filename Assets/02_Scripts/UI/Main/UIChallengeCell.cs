using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChallengeCell : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subTitleText;
    [SerializeField] private List<UICell_Reward> cellRewardList;
    [SerializeField] private GameObject lockObject;
    [SerializeField] private TextMeshProUGUI lockText;

    public void SetData(int _stageID, System.Action _startAction)
    {
        var stageInfo = DataManager.Instance.GetStageInfoData(_stageID);
        var rewards = DataManager.Instance.GetStageRewards(_stageID);

        for (int i = 0; i < rewards.Count; i++)
        {
            cellRewardList[i].SetActive(true);
            cellRewardList[i].UpdateContent(new GridItemData(rewards[i].id));
        }

        for (int i = rewards.Count; i < cellRewardList.Count; i++)
        {
            cellRewardList[i].SetActive(false);
        }
        
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() =>
        {
            _startAction?.Invoke();
        });

        var stageStatus = UserData.Instance.GetStageStatus(_stageID);
        if (stageStatus == Game.StageStatus.Lock)
        {
            var priorStageInfo = DataManager.Instance.GetStageInfoData(stageInfo.priorstageid);
            lockText.SetText(LocalizeManager.Instance.GetLocalString(priorStageInfo.stagename));
            lockObject.SetActive(true);
        }
        else
        {
            lockObject.SetActive(false);
        }
        
    }
}
