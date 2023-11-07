using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelUpPopup : PopupBase
{
    [SerializeField] private GameObject addSlotObject;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI addGoldText;
    [SerializeField] private TextMeshProUGUI addSlotText;

    public override void InitPopup(Action _hideAction)
    {
        base.InitPopup(_hideAction);
    }

    public void SetData(int _level)
    {
        var prevLevelInfo = DataManager.Instance.GetLevelData(_level - 1);
        var currLevelInfo = DataManager.Instance.GetLevelData(_level);

        levelText.SetText(LocalizeManager.Instance.GetLocalString($"Level {_level}"));
        addGoldText.SetText( currLevelInfo.goldreward.GetCommaString());
        if (prevLevelInfo.unlockslot < currLevelInfo.unlockslot)
        {
            addSlotObject.SetActive(true);
            addSlotText.SetText($"+ {currLevelInfo.unlockslot - prevLevelInfo.unlockslot}");
        }
        else
        {
            addSlotObject.SetActive(false);
        }
    }

}
