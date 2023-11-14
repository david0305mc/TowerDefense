using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupBase : MonoBehaviour
{
    [SerializeField] private Animator openAnim = default;
    [SerializeField] private Button closeBtn = default;
    private System.Action hideAction;
    private System.Action completeOpenAni;

    protected virtual void Awake()
    {
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(() =>
            {
                OnClickCloseBtn();
            });
        }
    }
    public virtual void InitPopup(System.Action _hideAction)
    {
        hideAction = _hideAction;
        
        if (openAnim != null && openAnim.enabled)
        {
            openAnim.Update(0);
            //StartCoroutine(CompleteOpenAnim());
            
            UniTask.Create(async () => {
                await UniTask.WaitUntil(() => !openAnim.IsInTransition(0));
                await UniTask.WaitUntil(() => openAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
                completeOpenAni?.Invoke();
            });
        }
        else
        {
            completeOpenAni?.Invoke();
        }
    }

    public void Hide()
    {
        hideAction?.Invoke();
        hideAction = null;
    }

    public void OnClickCloseBtn()
    {
        Hide();
    }
    IEnumerator CompleteOpenAnim()
    {
        yield return new WaitUntil(() => !openAnim.IsInTransition(0));

        while (openAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            yield return null;

        completeOpenAni?.Invoke();
    }

}
