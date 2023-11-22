using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using Cysharp.Threading.Tasks;

public class DisasterObj : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI disasterTimeText;

    private CancellationTokenSource cts;
    private readonly long disasterTime = 60 * 60 * 24 * 5;


    private void OnEnable()
    {
        if (UserData.Instance.LocalData.SignUpTime + disasterTime <= GameTime.Get())
        {
            // Disater Already Came Up
            disasterTimeText.SetText(LocalizeManager.Instance.GetLocalString("Comming Soon"));
        }
        else
        {
            //PlayAnim("Idle");
            //WaitForReward();
            //WaitForReward();
            WaitForDiaster();
        }
    }

    private void OnDisable()
    {
        cts?.Cancel();
        cts = null;
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    public void WaitForDiaster()
    {
        if (cts != null)
        {
            cts?.Cancel();
        }
        cts = new CancellationTokenSource();

        UniTask.Create(async () =>
        {
            while (UserData.Instance.LocalData.SignUpTime + disasterTime > GameTime.Get())
            {
                var seconds = UserData.Instance.LocalData.SignUpTime + disasterTime - GameTime.Get();
                disasterTimeText.SetText(System.TimeSpan.FromSeconds(seconds).ToString(@"dd\:hh\:mm"));
                await UniTask.WaitForSeconds(0.1f, cancellationToken: cts.Token);
            }
            disasterTimeText.SetText(LocalizeManager.Instance.GetLocalString("Comming Soon"));
        });
    }
}
