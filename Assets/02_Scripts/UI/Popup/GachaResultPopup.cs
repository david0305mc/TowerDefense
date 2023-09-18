using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultPopup : PopupBase
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject showUpEffectObj;
    [SerializeField] private GameObject resultListObj;

    public void SetData()
    {
        animator.Update(0);
        //StartCoroutine(CompleteOpenAnim());

        UniTask.Create(async () => {
            for (int i = 0; i < 5; i++)
            {
                await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
                await UniTask.WaitUntil(() => !animator.IsInTransition(0));
                animator.SetTrigger("ShowUpEffect");
            }
            animator.SetTrigger("ShowResultList");
        });
    }

}
