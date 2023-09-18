using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaResultPopup : PopupBase
{
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI unitNameText;

    public void SetData(List<int> gachaList)
    {
        animator.Update(0);
        //StartCoroutine(CompleteOpenAnim());

        UniTask.Create(async () => {
            for (int i = 0; i < gachaList.Count; i++)
            {
                await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
                await UniTask.WaitUntil(() => !animator.IsInTransition(0));
                animator.SetTrigger("ShowUpEffect");
                var unitInfo = DataManager.Instance.GetUnitinfoData(gachaList[i]);
                unitNameText.SetText(unitInfo.unitname);
            }
            animator.SetTrigger("ShowResultList");
        });
    }

}
