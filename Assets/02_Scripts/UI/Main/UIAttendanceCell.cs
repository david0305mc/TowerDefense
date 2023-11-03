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

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void UpdateContent(GridItemData _itemData)
    {
        dayText.SetText($"Day {_itemData.id}");
        List<DataManager.Attendance> dataLists = DataManager.Instance.GetAttendanceInfosByDay(_itemData.id);
        for (int i = 0; i < dataLists.Count; i++)
        {
            uiRewardLists[i].SetActive(true);
            uiRewardLists[i].SetData(dataLists[i].rewardid, dataLists[i].rewardtype, dataLists[i].rewardcount);
        }

        for (int i = dataLists.Count; i < uiRewardLists.Count; i++)
        {
            uiRewardLists[i].SetActive(false);
        }
    }
}
