using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class UIStage : MonoBehaviour
{
    [SerializeField] private Transform goldTargetTR;
    public Transform GoldTarget => goldTargetTR;
    [SerializeField] private TextMeshProUGUI tileLeftText;
    [SerializeField] private TextMeshProUGUI acquireGoldText;
    private CompositeDisposable disposable = new CompositeDisposable();
    public void SetData(long endUnixTime, CancellationTokenSource _cts)
    {
        disposable.Clear();
        Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
       .Select(x => endUnixTime - GameTime.Get())
       .TakeWhile(s => s > 0)
       .Subscribe(timeLeft =>
       {
           var timeSpan = TimeSpan.FromSeconds(timeLeft);
           tileLeftText.SetText(timeSpan.ToString(@"mm\:ss"));
       },
       () =>
       {
           tileLeftText.SetText("00:00");
           MGameManager.Instance.LoseStage();
            // To Do : Result
       }).AddTo(_cts.Token);

        UserData.Instance.AcquireGold.Subscribe(_gold =>
        {
            acquireGoldText.SetText(_gold.ToString());
        }).AddTo(disposable);
    }
    private void OnDisable()
    {
        disposable.Clear();
    }
}

