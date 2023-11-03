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

        UIAttendanceCellData[] itemData = Enumerable.Range(0, 7).Select(i => new UIAttendanceCellData(i)).ToArray();
        gridView.UpdateContents(itemData);
        gridView.OnCellClicked(index =>
        {
        });
    }

}
