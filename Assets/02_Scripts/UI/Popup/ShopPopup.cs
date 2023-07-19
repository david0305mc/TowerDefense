using EasingCore;
using FancyScrollView.Example07;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopPopup : PopupBase
{
    [SerializeField] private Button closeBtn = default;
    [SerializeField] private UIGridView gridView = default;

    private ShopItemData[] shopItemDatas;
    private void Awake()
    {
        closeBtn.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Start()
    {
        shopItemDatas = DataManager.Instance.ObjtableDic.Select(i => new ShopItemData(i.Key)).ToArray();
        gridView.UpdateContents(shopItemDatas);

        gridView.OnCellClicked(index =>
        {
            SelectCell(index);
        });
    }
    private void OnDisable()
    {
        UserData.Instance.ShopSelectedItem = -1;
    }

    void SelectCell(int index)
    {
        if (gridView.DataCount == 0)
        {
            return;
        }

        gridView.UpdateSelection(index);
        gridView.ScrollTo(index, 0.4f, Ease.InOutQuint, Alignment.Middle);

        UserData.Instance.ShopSelectedItem = shopItemDatas[index].id;
    }

}
