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

    private void Awake()
    {
        closeBtn.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    private void Start()
    {
        //var items = Enumerable.Range(0, 20).Select(i => new ItemData($"Cell {i}")).ToArray();

        //gridView.UpdateSelection(1);
        //gridView.ScrollTo(1, 0.4f, Ease.InOutQuint, FancyScrollView.Example08.Alignment.Middle);

        //uiScrollView.UpdateData(items);
        //uiScrollView.SelectCell(0);
        GenerateCells(100);

        gridView.OnCellClicked(index =>
        {
            SelectCell(index);
        });
    }

    void GenerateCells(int dataCount)
    {
        var items = Enumerable.Range(0, dataCount)
            .Select(i => new GridItemData(i))
            .ToArray();

        gridView.UpdateContents(items);
        //SelectCell();
    }

    void SelectCell(int index)
    {
        if (gridView.DataCount == 0)
        {
            return;
        }

        gridView.UpdateSelection(index);
        gridView.ScrollTo(index, 0.4f, Ease.InOutQuint, Alignment.Middle);
    }

}
