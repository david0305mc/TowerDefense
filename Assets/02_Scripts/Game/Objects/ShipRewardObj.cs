using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ShipRewardObj : MonoBehaviour
{
    [SerializeField] private Animator skeletonGiftAnimator;
    private CancellationTokenSource cts;


    public void ReceiveReward()
    {
        PlayAnim("Open");
        WaitForReward();
    }
    public void PlayAnim(string _trigger)
    {
        skeletonGiftAnimator.SetTrigger(_trigger);
    }

    public void WaitForReward()
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();

        UniTask.Create(async () =>
        {
            while (UserData.Instance.LocalData.ShipRewardableTime > GameTime.Get())
            {
                await UniTask.WaitForSeconds(1, cancellationToken: cts.Token);
            }
            PlayAnim("Ready");
        });
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
    private void Start()
    {
        if (UserData.Instance.LocalData.ShipRewardableTime <= GameTime.Get())
        {
            PlayAnim("Ready");
        }
        else
        {
            PlayAnim("Idle");
            WaitForReward();
        }
    }
}
