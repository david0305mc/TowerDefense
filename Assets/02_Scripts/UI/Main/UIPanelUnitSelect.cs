using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class UIPanelUnitSelect : MonoBehaviour
{
    [SerializeField] private UIGridView gridView = default;
    [SerializeField] private List<UIBattlePartySlot> battlePartyList = default;

    private List<UnitData> heroDataList;

    private void Awake()
    {
        MessageDispather.Receive(EMessage.Update_HeroParty).Subscribe(_ =>
        {
            InitUI();
        }).AddTo(gameObject);
    }
    public void InitUI()
    {
        InitBattlePartyUI();
        InitUserListScroll();
    }

    private void InitUserListScroll()
    {
        heroDataList = (from item in UserData.Instance.LocalData.HeroDataDic
                        orderby item.Key ascending
                        orderby item.Value.grade descending
                        orderby item.Value.refData.unitrarity descending
                        select item.Value).ToList();
        UIUnitData[] itemData = Enumerable.Range(0, heroDataList.Count).Select(i => new UIUnitData(i, heroDataList[i].uid)).ToArray();
        gridView.UpdateContents(itemData);
        gridView.OnCellClicked(index =>
        {
            ShowUnitInfoPopup(index);
        });
    }

    public void ShowUnitInfoPopup(int _index)
    {
        UnitData heroData = heroDataList[_index];
        var popup = PopupManager.Instance.Show<UnitInfoPopup>();
        popup.SetData(heroData.uid, () =>
        {
            int partySlotIndex = UserData.Instance.GetPartySlotIndexByUID(heroData.uid);
            if (partySlotIndex == -1)
            {
                // Equip
                if (UserData.Instance.FindEmptySlot() == -1)
                {
                    PopupManager.Instance.ShowSystemOneBtnPopup("No Empty Slot", "OK");
                    return;
                }
                int slotIndex = MGameManager.Instance.AddBattleParty(heroData.uid);
            }
            else
            {
                // UnEquip
                MGameManager.Instance.RemoveBattleParty(partySlotIndex);
            }
        });
    }
    private void InitBattlePartyUI()
    {
        Enumerable.Range(0, battlePartyList.Count).ToList().ForEach(i =>
        {
            int unitUID = UserData.Instance.GetBattlePartyUIDByIndex(i);
            battlePartyList[i].SetData(i, unitUID, (_slotIndex) =>
            {
                MGameManager.Instance.RemoveBattleParty(_slotIndex);
            }); 
        });
    }

    private void ClearPool()
    {
        foreach (var item in battlePartyList)
        {
            item.ClearPool();
        }
    }
    private void OnDisable()
    {
        ClearPool();
    }

}
