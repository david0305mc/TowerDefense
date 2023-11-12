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
    [SerializeField] private Image unitIconImage;
    [SerializeField] private Image unitBGImage;
    [SerializeField] private Image textRarityBG;
    [SerializeField] private TextMeshProUGUI euipToggleText;
    [SerializeField] private TextMeshProUGUI heroNameText;
    [SerializeField] private TextMeshProUGUI unitrarityText;
    [SerializeField] private TextMeshProUGUI combatPowerText;
    [SerializeField] private TextMeshProUGUI nextCombatPowerText;
    [SerializeField] private TextMeshProUGUI maxCombatPowerText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private GameObject combatDefaltObj;
    [SerializeField] private GameObject combatMaxObj;

    [SerializeField] private UnitGradeInfo unitGradeInfo;

    [SerializeField] private TextMeshProUGUI upgradeCostText;

    private System.Action equipAction;
    private System.Action unEquipAction;
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
        heroNameText.SetText(LocalizeManager.Instance.GetLocalString(unitData.refData.unitname));
        UpdateUI();

        if (UserData.Instance.LocalData.CurrTutorialID == 15)
        {
            MGameManager.Instance.PlayNextTutorial(() => {
                equipAction?.Invoke();
                Hide();
            });
        }
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
        unitIconImage.sprite = MResourceManager.Instance.GetSpriteFromAtlas(unitData.refData.thumbnailpath);
        textRarityBG.color = MResourceManager.Instance.GetRarityColor(unitData.refData.unitrarity);
        unitBGImage.sprite = MResourceManager.Instance.GetBuildAtlas($"RatingBG_{(int)unitData.refData.unitrarity}");
     
        hpText.SetText(unitData.refUnitGradeData.hp.ToString());
        int damage = unitData.refUnitGradeData.attackdmg * unitData.refUnitGradeData.attackcount + unitData.refUnitGradeData.splashdmg;
        damageText.SetText(damage.ToString());
        countText.SetText(unitData.refUnitGradeData.summoncnt.ToString());

        if (unitData.IsMaxGrade)
        {
            combatDefaltObj.SetActive(false);
            combatMaxObj.SetActive(true);
            maxCombatPowerText.SetText(unitData.refUnitGradeData.combatpower.ToString());
            upgradeBtn.SetActive(false);
            maxUpgradeBtn.SetActive(true);
        }
        else
        {
            combatDefaltObj.SetActive(true);
            combatMaxObj.SetActive(false);
            upgradeBtn.SetActive(true);
            maxUpgradeBtn.SetActive(false);
            upgradeCostText.SetText(unitData.refUnitGradeData.upgradecostcnt.ToString());
            
            int nextPower = DataManager.Instance.GetUnitGrade(unitData.tid, unitData.grade + 1).combatpower;
            combatPowerText.SetText(unitData.refUnitGradeData.combatpower.ToString());
            nextCombatPowerText.SetText(nextPower.ToString());
        }
    }

}
