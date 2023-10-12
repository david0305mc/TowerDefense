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
        if (itemData.id == 0)
        {
            // index 0 : Soul
            thumbnail.sprite = MResourceManager.Instance.SoulSprite;
            countText.SetText(UserData.Instance.AcquireSoul.Value.ToString());
        }
        else
        {
            var rewardInfo = DataManager.Instance.GetStageRewardInfoData(itemData.id);
            rewardInfo.rewardcount.ToString();
            switch (rewardInfo.rewardtype)
            {
                case ITEM_TYPE.UNIT:
                    {
                        var unitInfo = DataManager.Instance.GetUnitinfoData(rewardInfo.rewardid);
                        thumbnail.sprite = MResourceManager.Instance.GetSpriteFromAtlas(unitInfo.thumbnailpath);
                        countText.SetText(rewardInfo.rewardcount.ToString());
                    }
                    break;
                case ITEM_TYPE.SOUL:
                    {
                        thumbnail.sprite = MResourceManager.Instance.SoulSprite;
                        countText.SetText(rewardInfo.rewardcount.ToString());
                    }
                    break;
                case ITEM_TYPE.EXP:
                    {
                        thumbnail.sprite = MResourceManager.Instance.ExpSprite;
                        countText.SetText(rewardInfo.rewardcount.ToString());
                    }
                    break;
            }
        }
    }
}
