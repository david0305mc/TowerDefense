using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIGacharesultData : GridItemData
{
    public UIGacharesultData(int _index) : base(_index)
    {
    }
}

public class UIGachaResultCell : UIGridCell
{
    [SerializeField] private Image thumbnailBG;
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI countText;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void UpdateContent(GridItemData itemData)
    {
        var gachaInfo = DataManager.Instance.GetGachaListData(itemData.id);
        countText.SetText(gachaInfo.count.ToString());
        var unitInfo = DataManager.Instance.GetUnitinfoData(gachaInfo.unitid);
        thumbnail.sprite = MResourceManager.Instance.GetSpriteFromAtlas(unitInfo.thumbnailpath);
        thumbnailBG.sprite = MResourceManager.Instance.GetBuildAtlas($"RatingBG_{(int)unitInfo.unitrarity}");
    }
}
