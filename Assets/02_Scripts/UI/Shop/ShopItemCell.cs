using FancyScrollView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopItemCell : UIGridCell
{
    [SerializeField] private TextMeshProUGUI text;
    public override void UpdateContent(GridItemData itemData)
    {
        text.SetText(itemData.Index.ToString());
    }
}
