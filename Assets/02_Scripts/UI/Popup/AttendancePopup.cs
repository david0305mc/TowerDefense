using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttendancePopup : PopupBase
{
    [SerializeField] private UIGridView gridView = default;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void InitPopup(Action _hideAction)
    {
        base.InitPopup(_hideAction);

        int maxDay = DataManager.Instance.AttendanceDic.Max(item => item.Value.day);
        UIAttendanceCellData[] itemData = Enumerable.Range(1, maxDay).Select(i => new UIAttendanceCellData(i)).ToArray();
        gridView.UpdateContents(itemData);
        gridView.OnCellClicked(index =>
        {
            Debug.Log("OnCellClicked");
        });
    }

}
