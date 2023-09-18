using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIGacharesultData : GridItemData
{
    public int count;
    public UIGacharesultData(int _index, int _count) : base(_index)
    {
        count = _count;
    }
}

public class UIGachaResultCell : UIGridCell
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI countText;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void UpdateContent(GridItemData itemData)
    {
        UIGacharesultData data = (UIGacharesultData)itemData;

        countText.SetText(data.count.ToString());
        Debug.Log($"UpdateContent {itemData.id}");
    }
}
