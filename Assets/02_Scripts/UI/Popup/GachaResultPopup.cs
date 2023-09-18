using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;

public class GachaResultPopup : PopupBase
{
    [SerializeField] private Button showNextBtn;
    [SerializeField] private Button skipToLastFrameBtn;
    [SerializeField] private Button skipToResultBtn;
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI unitNameText;

    private List<int> gachaList;
    private CancellationTokenSource cts;
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
    }

    public void SetData(List<int> _gachaList)
    {
        gachaList = _gachaList;
        index = 0;
        ShowUpEffect();

        //if (cts != null)
        //    cts.Cancel();
        //cts = new CancellationTokenSource();

        //StartCoroutine(CompleteOpenAnim());

        //UniTask.Create(async () => {
        //    for (int i = 0; i < gachaList.Count; i++)
        //    {
        //        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f, cancellationToken: cts.Token);
        //        await UniTask.WaitUntil(() => !animator.IsInTransition(0), cancellationToken:cts.Token);
        //    }
        //    animator.SetTrigger("ShowResultList");
            
        //});
    }



}
