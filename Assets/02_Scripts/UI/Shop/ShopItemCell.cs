using FancyScrollView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ShopItemData : GridItemData
{
    public ShopItemData(int index) : base(index)
    {

    }
}

public class ShopItemCell : UIGridCell
{
    [SerializeField] private Image thumbNail;
    public override void UpdateContent(GridItemData itemData)
    {
        var objInfo = DataManager.Instance.GetObjTableData(itemData.Index);
        thumbNail.sprite = Utill.Load<Sprite>(objInfo.thumbnailpath);
    }
}
