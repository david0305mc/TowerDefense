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
        MessageDispather.Receive(EMessage.Update_UserData).Subscribe(_ => {
            InitUI(); 
        });
    }
    public void InitUI()
    {
        InitUserListScroll();
        InitBattlePartyUI();
    }

    private void InitUserListScroll()
    {
        heroDataList = UserData.Instance.LocalData.HeroDataDic.Values.ToList();
        UIUnitData[] itemData = Enumerable.Range(0, heroDataList.Count).Select(i => new UIUnitData(heroDataList[i].uid)).ToArray();
        gridView.UpdateContents(itemData);
        gridView.OnCellClicked(index =>
        {
            UnitData heroData = heroDataList[index];
            var popup = PopupManager.Instance.Show<UnitInfoPopup>();
            popup.SetData(heroData.uid, () =>
            {
                if (UserData.Instance.FindEmptySlot() == -1)
                {
                    PopupManager.Instance.ShowSystemOneBtnPopup("No Empty Slot", "OK");
                    return;
                }

                int partySlotIndex = UserData.Instance.GetPartySlotIndexByUID(heroData.uid);
                if (partySlotIndex == -1)
                {
                    int slotIndex = MGameManager.Instance.AddBattleParty(heroData.uid);
                    //battlePartyList[slotIndex].AddHero(heroData.uid);
                    //InitUserListScroll();
                }
                else
                {
                    MGameManager.Instance.RemoveBattleParty(partySlotIndex);
                    //battlePartyList[partySlotIndex].RemoveHero();
                    //InitUserListScroll();
                }
            });
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
