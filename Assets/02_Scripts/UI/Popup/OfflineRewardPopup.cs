using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineRewardPopup : PopupBase
{
    [SerializeField] private MButton receiveBtn;

    protected override void Awake()
    {
        base.Awake(); 
    }

    public void SetData()
    {
        receiveBtn.onClick.RemoveAllListeners();
        receiveBtn.onClick.AddListener(() =>
        {
            Hide();
        });
    }
}
