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
                case ITEM_TYPE.GOLD:
                    {
                        thumbnail.sprite = MResourceManager.Instance.GoldSprite;
                        countText.SetText(rewardInfo.rewardcount.ToString());
                    }
                    break;
            }
        }
    }

    public void SetData(int _itemID, ITEM_TYPE _itemType, int _count)
    {
        switch (_itemType)
        {
            case ITEM_TYPE.UNIT:
                {
                    var unitInfo = DataManager.Instance.GetUnitinfoData(_itemID);
                    thumbnail.sprite = MResourceManager.Instance.GetSpriteFromAtlas(unitInfo.thumbnailpath);
                    countText.SetText(_count.ToString());
                }
                break;
            case ITEM_TYPE.SOUL:
                {
                    thumbnail.sprite = MResourceManager.Instance.SoulSprite;
                    countText.SetText(_count.ToString());
                }
                break;
            case ITEM_TYPE.EXP:
                {
                    thumbnail.sprite = MResourceManager.Instance.ExpSprite;
                    countText.SetText(_count.ToString());
                }
                break;
            case ITEM_TYPE.STAMINA:
                {
                    thumbnail.sprite = MResourceManager.Instance.StaminaSprite;
                    countText.SetText(_count.ToString());
                }
                break;
            case ITEM_TYPE.GOLD:
                {
                    thumbnail.sprite = MResourceManager.Instance.GoldSprite;
                    countText.SetText(_count.ToString());
                }
                break;
        }
    }
}
