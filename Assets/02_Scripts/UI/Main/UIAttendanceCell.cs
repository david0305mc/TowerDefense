using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;


public class UIAttendanceCellData : GridItemData
{
    public UIAttendanceCellData(int _index) : base(_index)
    {
 
    }
}

public class UIAttendanceCell : UIGridCell
{

    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject checkerObject;
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
        //UIUnitData itemData = (UIUnitData)_itemData;
        //var selected = Context.SelectedIndex == Index;
        //var heroData = UserData.Instance.GetHeroData(itemData.uid);
        //checkerObject.SetActive(UserData.Instance.GetPartySlotIndexByUID(heroData.uid) != -1);
        //iconImage.sprite = MResourceManager.Instance.GetSpriteFromAtlas(heroData.refData.thumbnailpath);
        //unitGradeInfo.SetData(heroData.grade, heroData.IsMaxGrade, heroData.count, heroData.refUnitGradeData.upgradepiececnt);

        //if (UserData.Instance.LocalData.CurrTutorialID == 14)
        //{
        //    if (itemData.id == 1)
        //    {
        //        UniTask.Create(async () =>
        //        {
        //            await UniTask.Yield();
        //            TutorialTouchEvent obj = iconImage.AddComponent<TutorialTouchEvent>();
        //            obj.TutorialID = 14;
        //            MGameManager.Instance.PlayNextTutorial();
        //        });
        //    }
        //}
    }
}
