using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardPopup : PopupBase
{
    [SerializeField] protected List<UICell_Reward> uiRewardList;
    public void SetData(List<RewardData> _rewardList)
    {
        for (int i = 0; i < _rewardList.Count; i++)
        {
            uiRewardList[i].SetActive(true);
            uiRewardList[i].SetData(_rewardList[i].rewardid, _rewardList[i].rewardtype, _rewardList[i].rewardcount);
        }

        for (int i = _rewardList.Count; i < uiRewardList.Count; i++)
        {
            uiRewardList[i].SetActive(false);
        }
    }

}
