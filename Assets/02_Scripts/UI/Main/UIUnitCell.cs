using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FancyScrollView;
using UnityEngine.UI;

public class UIUnitData  : GridItemData
{
    public UIUnitData(int index) : base(index)
    { 
    
    }
}

public class UIUnitCell : UIGridCell
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject checkerObject;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI countText;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void SetData(int _uid)
    {

    }

    public override void UpdateContent(GridItemData itemData)
    {
        Debug.Log($"UpdateContent {itemData.id}");
        var selected = Context.SelectedIndex == Index;
        var heroData = UserData.Instance.GetHeroData(itemData.id);
        checkerObject.SetActive(UserData.Instance.GetPartySlotIndexByUID(heroData.uid) != -1);
    }
}
