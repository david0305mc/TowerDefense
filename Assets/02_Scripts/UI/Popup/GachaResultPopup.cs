using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GachaResultPopup : PopupBase
{
    [SerializeField] private Button showNextBtn;
    [SerializeField] private Button skipToLastFrameBtn;
    [SerializeField] private Button skipToResultBtn;
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private Transform unitPos;

    [SerializeField] private Image unitRarityImage;
    [SerializeField] private TextMeshProUGUI unitrarityText;
    [SerializeField] private UnitGradeInfo unitGradeInfo;

    [SerializeField] private UIGridView resultScrollView = default;

    private MHeroObj heroObj = default;

    private List<int> gachaList;
    private int index;

    protected override void Awake()
    {
        base.Awake();
        showNextBtn.onClick.AddListener(() =>
        {
            index++;
            if (index < gachaList.Count)
            {
                ShowUpEffect();
            }
            else
            {
                ShowResultList();
            }
        });
        skipToLastFrameBtn.onClick.AddListener(() =>
        {
            animator.Play("GachaShowUpEffect", 0, 1f);
        });
        skipToResultBtn.onClick.AddListener(() =>
        {
            ShowResultList();
        });
    }

    private void ClearPool()
    {
        if (heroObj != null)
        {
            Lean.Pool.LeanPool.Despawn(heroObj.gameObject);
        }
    }
    private void OnDisable()
    {
        ClearPool();
    }
    private void ShowUpEffect()
    {
        ClearPool();
        animator.SetTrigger("ShowUpEffect");
        int grade = 1;
        var gachaInfo = DataManager.Instance.GetGachaListData(gachaList[index]);
        var unitData = UserData.Instance.GetHeroDataByTID(gachaInfo.unitid);
        //var unitInfo = DataManager.Instance.GetUnitinfoData(gachaInfo.unitid);
        //var unitGradeInfo = DataManager.Instance.GetUnitGrade(gachaInfo.unitid, grade);

        GameObject unitPrefab = MResourceManager.Instance.GetPrefab(unitData.refData.prefabname);
        heroObj = Lean.Pool.LeanPool.Spawn(unitPrefab, Vector3.zero, Quaternion.identity, unitPos).GetComponent<MHeroObj>();
        heroObj.transform.SetLocalPosition(Vector3.zero);
        heroObj.SetUIMode(Game.GameConfig.CanvasPopupManagerLayerOrder + index + 2);
        
        unitNameText.SetText(LocalizeManager.Instance.GetLocalString(unitData.refData.unitname));
        unitGradeInfo.SetData(unitData.grade, unitData.IsMaxGrade, unitData.count, unitData.refUnitGradeData.upgradepiececnt);
        unitrarityText.SetText(unitData.refData.unitrarity.GetEnumLocalization());
        unitRarityImage.color = MResourceManager.Instance.GetRarityColor(unitData.refData.unitrarity);
        //rarityBG.sprite = MResourceManager.Instance.GetBuildAtlas($"RatingBG_{(int)unitData.refData.unitrarity}");
    }

    private void ShowResultList()
    {
        animator.SetTrigger("ShowResultList");
        SpawnScrollItem();
    }


    private void SpawnScrollItem()
    {
        UIGacharesultData[] itemData = Enumerable.Range(0, gachaList.Count).Select(i => new UIGacharesultData(gachaList[i])).ToArray();
        resultScrollView.UpdateContents(itemData);
        resultScrollView.OnCellClicked(index =>
        {
        });
    }


    public void SetData(List<int> _gachaList)
    {
        gachaList = _gachaList;
        index = 0;
        ShowUpEffect();
    }



}
