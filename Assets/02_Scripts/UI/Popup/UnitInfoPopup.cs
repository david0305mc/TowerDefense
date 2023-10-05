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
    [SerializeField] private Button maxUpgradeBtn;
    [SerializeField] private Image rarityBG;
    [SerializeField] private TextMeshProUGUI euipToggleText;
    [SerializeField] private TextMeshProUGUI heroNameText;
    [SerializeField] private TextMeshProUGUI unitrarityText;
    [SerializeField] private TextMeshProUGUI combatPowerText;
    [SerializeField] private TextMeshProUGUI nextCombatPowerText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI countText;

    [SerializeField] private Image unitRarityImage;
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
        unitrarityText.SetText(unitData.refData.unitrarity.GetEnumLocalization());
        unitRarityImage.color = MResourceManager.Instance.GetRarityColor(unitData.refData.unitrarity);
        rarityBG.sprite = MResourceManager.Instance.GetBuildAtlas($"RatingBG_{(int)unitData.refData.unitrarity}");
        combatPowerText.SetText(unitData.refUnitGradeData.combatpower.ToString());
     
        hpText.SetText(unitData.refUnitGradeData.hp.ToString());
        damageText.SetText(unitData.refUnitGradeData.attackdmg.ToString());
        countText.SetText(unitData.count.ToString());

        if (unitData.IsMaxGrade)
        {
            nextCombatPowerText.SetText(LocalizeManager.Instance.GetLocalString("MaxGradeText"));
            upgradeBtn.SetActive(false);
            maxUpgradeBtn.SetActive(true);
        }
        else
        {
            upgradeBtn.SetActive(true);
            maxUpgradeBtn.SetActive(false);
            upgradeCostText.SetText(unitData.refUnitGradeData.upgradecostcnt.ToString());
            int nextPower = DataManager.Instance.GetUnitGrade(unitData.uid, unitData.grade + 1).combatpower;
            nextCombatPowerText.SetText(nextPower.ToString());
        }
    }

}
