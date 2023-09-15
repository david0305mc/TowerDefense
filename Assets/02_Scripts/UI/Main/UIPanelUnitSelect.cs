using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPanelUnitSelect : MonoBehaviour
{
    [SerializeField] private UIGridView gridView = default;
    [SerializeField] private List<UIBattlePartySlot> battlePartyList = default;

    private List<UnitData> heroDataList;
    
    public void InitUI()
    {
        InitUserListScroll();
        InitBattlePartyUI();
    }

    private void InitUserListScroll()
    {
        heroDataList = UserData.Instance.heroDataDic.Values.ToList();
        UIUnitData[] itemData = Enumerable.Range(0, heroDataList.Count).Select(i => new UIUnitData(heroDataList[i].uid)).ToArray();
        gridView.UpdateContents(itemData);
        gridView.OnCellClicked(index =>
        {
            UnitData heroData = heroDataList[index];
            int battleSlotIndex = UserData.Instance.GetBattleSlotIndexByUID(heroData.uid);

            if (battleSlotIndex == -1)
            {
                int slotIndex = UserData.Instance.AddBattleParty(heroData.uid);
                battlePartyList[slotIndex].AddHero(heroData.uid);
                InitUserListScroll();
            }
            else
            {
                int slotIndex = UserData.Instance.GetBattleSlotIndexByUID(heroData.uid);
                UserData.Instance.RemoveBattleParty(slotIndex);
                battlePartyList[slotIndex].RemoveHero();
                InitUserListScroll();
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
                UserData.Instance.RemoveBattleParty(_slotIndex);
                battlePartyList[i].RemoveHero();
                InitUserListScroll();
            }); 
        });
    }

    private void OnDisable()
    {
        foreach (var item in battlePartyList)
        {
            item.ClearPool();
        }
    }

}
