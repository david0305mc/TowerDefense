using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OptionPopup : PopupBase
{
    [SerializeField] private List<UILanguageCell> uilanguageCellLists;

    public override void InitPopup(Action _hideAction)
    {
        base.InitPopup(_hideAction);
        UpdateUI();
    }

    private void UpdateUI()
    {
        foreach (var item in uilanguageCellLists)
        {
            item.SetData(_lang =>
            {
                LocalizeManager.Instance.Language = _lang;
            });
        }
    }

    private void OnLocalize()
    {
        UpdateUI();
    }
}
