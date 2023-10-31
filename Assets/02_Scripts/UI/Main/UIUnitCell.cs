using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FancyScrollView;
using UnityEngine.UI;

public class UIUnitData  : GridItemData
{
    public int uid;
    public UIUnitData(int _index, int _uid) : base(_index)
    {
        uid = _uid;
    }
}

public class UIUnitCell : UIGridCell
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject checkerObject;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private UnitGradeInfo unitGradeInfo;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void SetData(int _uid)
    {

    }

    public override void UpdateContent(GridItemData _itemData)
    {
        UIUnitData itemData = (UIUnitData)_itemData;
        var selected = Context.SelectedIndex == Index;
        var heroData = UserData.Instance.GetHeroData(itemData.uid);
        checkerObject.SetActive(UserData.Instance.GetPartySlotIndexByUID(heroData.uid) != -1);
        iconImage.sprite = MResourceManager.Instance.GetSpriteFromAtlas(heroData.refData.thumbnailpath);
        unitGradeInfo.SetData(heroData.grade, heroData.IsMaxGrade, heroData.count, heroData.refUnitGradeData.upgradepiececnt);

        if (UserData.Instance.LocalData.CurrTutorialID == 14)
        {
            if (itemData.id == 1)
            {
                TutorialTouchEvent obj = gameObject.AddComponent<TutorialTouchEvent>();
                obj.TutorialID = 14;
            }
            MGameManager.Instance.PlayNextTutorial();
        }
    }
}
