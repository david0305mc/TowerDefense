using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

public class AttendancePopup : PopupBase
{
    [SerializeField] private UIGridView gridView = default;

    protected override void Awake()
    {
        base.Awake();
        MessageDispather.Receive(EMessage.Update_Attendance).Subscribe(_ =>
        {
            UpdateUI();
        }).AddTo(gameObject);
    }

    public override void InitPopup(Action _hideAction)
    {
        base.InitPopup(_hideAction);
        UserData.Instance.CheckAttendance();
        UpdateUI();
    }

    private void UpdateUI()
    {
        int maxDay = DataManager.Instance.AttendanceDic.Max(item => item.Value.day);
        UIAttendanceCellData[] itemData = Enumerable.Range(1, maxDay).Select(i => new UIAttendanceCellData(i)).ToArray();
        gridView.UpdateContents(itemData);
        gridView.OnCellClicked(index =>
        {
            int prevLevel = UserData.Instance.LocalData.Level.Value;
            MGameManager.Instance.ReceiveAttendanceReward(index + 1);
            int currLevel = UserData.Instance.LocalData.Level.Value;
            if (prevLevel < currLevel)
            {
                TouchBlockManager.Instance.AddLock();
                UniTask.Create(async () =>
                {
                    await UniTask.WaitForSeconds(0.3f);
                    var popup = PopupManager.Instance.Show<LevelUpPopup>();
                    popup.SetData(currLevel);
                    TouchBlockManager.Instance.RemoveLock();
                });
            }
        });
    }

}
