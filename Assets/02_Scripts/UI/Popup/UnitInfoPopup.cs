using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class UnitInfoPopup : PopupBase
{

    [SerializeField] private Button dimmBtn;
    [SerializeField] private Button euipToggleBtn;
    [SerializeField] private Button upgradeBtn;
    [SerializeField] private TextMeshProUGUI euipToggleText;
    [SerializeField] private TextMeshProUGUI heroNameText;
    [SerializeField] private UnitGradeInfo unitGradeInfo;

    [SerializeField] private TextMeshProUGUI upgradeCostText;

    private System.Action equipAction;
    private UnitData unitData;

    protected override void Awake()
    {
        base.Awake();
        dimmBtn.onClick.AddListener(() =>
        {
            OnClickCloseBtn();
        });
        euipToggleBtn.onClick.AddListener(() =>
        {
            equipAction?.Invoke();
            UpdateUI();
            Hide();
        });
        upgradeBtn.onClick.AddListener(() =>
        {
            MGameManager.Instance.UpgradeUnit(unitData.uid);
            UpdateUI();
        });
    }

    public void SetData(int _heroUID, System.Action _equipAction)
    {
        unitData = UserData.Instance.GetHeroData(_heroUID);
        equipAction = _equipAction;
        heroNameText.SetText(unitData.refData.unitname);
        UpdateUI();    
    }

    private void UpdateUI()
    {
        //UserData.Instance.FindEmptySlot();
        int slotIndex = UserData.Instance.GetPartySlotIndexByUID(unitData.uid);
        if (slotIndex == -1)
        {
            euipToggleText.SetText("Equip");
        }
        else
        {
            euipToggleText.SetText("UnEquip");
        }

        unitGradeInfo.SetData(unitData.grade, unitData.IsMaxGrade, unitData.count, unitData.refUnitGradeData.upgradepiececnt);
        upgradeCostText.SetText(unitData.refUnitGradeData.upgradecostcnt.ToString());
    }

}
