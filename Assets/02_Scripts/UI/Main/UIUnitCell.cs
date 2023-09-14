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
    [SerializeField] private TextMeshProUGUI uidText;
    [SerializeField] private TextMeshProUGUI tidText;

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
    }
}
