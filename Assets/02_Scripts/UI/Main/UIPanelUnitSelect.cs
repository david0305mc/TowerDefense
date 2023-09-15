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
        heroDataList = UserData.Instance.heroDataDic.Values.ToList();
        var itemData = Enumerable.Range(0, heroDataList.Count).Select(i => new UIUnitData(heroDataList[i].uid)).ToArray();
        gridView.UpdateContents(itemData);
        gridView.OnCellClicked(index =>
        {
            UnitData heroData = heroDataList[index];
            if (!heroData.isInParty)
            {
                int slotIndex = UserData.Instance.AddBattleParty(heroData.uid);
                battlePartyList[slotIndex].AddHero(heroData.uid);
                //heroObj.InitObject(heroData.uid, false, (_attackData) =>
                //{
                //    //DoHeroGetDamage(heroObj, _attackData.attackerUID, _attackData.damage);
                //});
                //heroObj.StartFSM();
                //heroDic.Add(heroData.uid, heroObj);
            }
            //SelectCell(index);
        });
        InitBattlePartyUI();
    }

    private void InitBattlePartyUI()
    {
        Enumerable.Range(0, battlePartyList.Count).ToList().ForEach(i =>
        {
            int unitUID = UserData.Instance.BattlePartyDic[i];
            battlePartyList[i].SetData(i, unitUID, (_slotIndex) =>
            {
                UserData.Instance.RemoveBattleParty(_slotIndex);
                battlePartyList[i].RemoveHero();
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
