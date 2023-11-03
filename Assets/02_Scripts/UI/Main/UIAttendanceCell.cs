using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;


public class UIAttendanceCellData : GridItemData
{
    public UIAttendanceCellData(int _index) : base(_index)
    {
 
    }
}

public class UIAttendanceCell : UIGridCell
{
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private List<UICell_Reward>  uiRewardLists;
    [SerializeField] private GameObject getButton;
    [SerializeField] private GameObject unactiveButton;
    [SerializeField] private GameObject rewardedButton;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void UpdateContent(GridItemData _itemData)
    {
        int day = _itemData.id;
        dayText.SetText($"Day {day}");
        List<DataManager.Attendance> dataLists = DataManager.Instance.GetAttendanceInfosByDay(day);
        for (int i = 0; i < dataLists.Count; i++)
        {
            uiRewardLists[i].SetActive(true);
            uiRewardLists[i].SetData(dataLists[i].rewardid, dataLists[i].rewardtype, dataLists[i].rewardcount);
        }

        for (int i = dataLists.Count; i < uiRewardLists.Count; i++)
        {
            uiRewardLists[i].SetActive(false);
        }

        if (UserData.Instance.LocalData.AttendanceDay >= day)
        {
            if (UserData.Instance.LocalData.AttendanceRewardedDic.ContainsKey(day) && UserData.Instance.LocalData.AttendanceRewardedDic[day] == 1)
            {
                rewardedButton.SetActive(true);
                getButton.SetActive(false);
                unactiveButton.SetActive(false);
            }
            else
            {
                rewardedButton.SetActive(false);
                getButton.SetActive(true);
                unactiveButton.SetActive(false);
            }
        }
        else 
        {
            rewardedButton.SetActive(false);
            getButton.SetActive(false);
            unactiveButton.SetActive(true);
        }
    }
}
