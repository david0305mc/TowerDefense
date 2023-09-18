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

    [SerializeField] private UIGridView resultScrollView = default;

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

    private void ShowUpEffect()
    {
        animator.SetTrigger("ShowUpEffect");
        var unitInfo = DataManager.Instance.GetUnitinfoData(gachaList[index]);
        unitNameText.SetText(unitInfo.unitname);
    }

    private void ShowResultList()
    {
        animator.SetTrigger("ShowResultList");
        SpawnScrollItem();
    }


    private void SpawnScrollItem()
    {
        UIGacharesultData[] itemData = Enumerable.Range(0, gachaList.Count).Select(i => new UIGacharesultData(gachaList[i], 3)).ToArray();
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
