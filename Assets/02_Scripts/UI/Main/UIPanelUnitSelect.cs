using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPanelUnitSelect : MonoBehaviour
{
    [SerializeField] private UIGridView gridView = default;
    [SerializeField] private List<UIBattlePartySlot> battlePartyList = default;

    private List<UnitData> heroList;
    void Start()
    {
        UpdateData();
    }

    private void UpdateData()
    {
        heroList = UserData.Instance.heroDataDic.Values.ToList();
        var itemData = Enumerable.Range(0, heroList.Count).Select(i => new UIUnitData(heroList[i].uid)).ToArray();
        gridView.UpdateContents(itemData);
        gridView.OnCellClicked(index =>
        {
            if (!heroList[index].isInParty)
            {
                int slotIndex = UserData.Instance.AddBattleParty(heroList[index].uid);
                battlePartyList[slotIndex].SetData(heroList[index].uid);
                Debug.Log($"OnCellClicked {index}");
            }
            //SelectCell(index);
        });
    }

}
