using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;
using System;

public class UIStage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tileLeftText;
    private CompositeDisposable disposable = new CompositeDisposable();
    public void SetData(long endUnixTime)
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
       }).AddTo(gameObject);
        //UniRx.Observable.Timer()

    }
    private void OnDisable()
    {
        disposable.Clear();
    }
}

