using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRewardCellData : GridItemData
{
    public UIRewardCellData(int _index) : base(_index)
    {
    }
}

public class UICell_Reward : UIGridCell
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI countText;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void UpdateContent(GridItemData itemData)
    {
        //var gachaInfo = DataManager.Instance.GetGachaListData(itemData.id);
        //countText.SetText(gachaInfo.count.ToString());
        //var unitInfo = DataManager.Instance.GetUnitinfoData(gachaInfo.unitid);
        //thumbnail.sprite = MResourceManager.Instance.GetSpriteFromAtlas(unitInfo.thumbnailpath);
    }
}
