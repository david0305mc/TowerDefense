using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPanelUnitSelect : MonoBehaviour
{
    [SerializeField] private UIGridView gridView = default;
    [SerializeField] private List<UIBattlePartySlot> battlePartyList = default;

    private List<UnitData> heroDataList;
    void Start()
    {
        UpdateData();
    }

    private void UpdateData()
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
                battlePartyList[slotIndex].SetData(heroData.uid);
                Debug.Log($"OnCellClicked {index}");

                //var heroData = UserData.Instance.GetHeroData(unitUid);
                
                GameObject unitPrefab = MResourceManager.Instance.GetPrefab(heroData.refData.prefabname);
                MHeroObj heroObj = Lean.Pool.LeanPool.Spawn(unitPrefab, Vector3.zero, Quaternion.identity, battlePartyList[slotIndex].CharacterViewTR).GetComponent<MHeroObj>();
                heroObj.transform.SetLocalPosition(Vector3.zero);
                heroObj.transform.SetScale(200);
                //heroObj.InitObject(heroData.uid, false, (_attackData) =>
                //{
                //    //DoHeroGetDamage(heroObj, _attackData.attackerUID, _attackData.damage);
                //});
                //heroObj.StartFSM();
                //heroDic.Add(heroData.uid, heroObj);
            }
            //SelectCell(index);
        });
    }

}
