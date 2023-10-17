using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public async UniTask PlayLoadingUIAsync()
    {
        await UniTask.WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    }

}
