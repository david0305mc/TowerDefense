using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class WorldmapLockObj : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI disasterTimeText;

    private CancellationTokenSource cts;
    private readonly long disasterTime = 60 * 60 * 24 * 4;


    private void OnEnable()
    {
        if (UserData.Instance.LocalData.SignUpTime + disasterTime <= GameTime.Get())
        {
            gameObject.SetActive(false);
        }
        else
        {
            WaitForUnlock();
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

    public void WaitForUnlock()
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
                disasterTimeText.SetText(GameUtil.ConvertSecondsToTimeleft(seconds));
                await UniTask.WaitForSeconds(0.1f, cancellationToken: cts.Token);
            }
            gameObject.SetActive(false);
        });
    }
}
