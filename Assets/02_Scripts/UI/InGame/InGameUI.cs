using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private Transform soulTargetTR;
    public Transform SoulTarget => soulTargetTR;
    [SerializeField] private TextMeshProUGUI tileLeftText;
    [SerializeField] private TextMeshProUGUI acquireSoulText;
    [SerializeField] private GameObject speedIconX1;
    [SerializeField] private GameObject speedIconX2;
    [SerializeField] private GameObject speedIconX4;
    [SerializeField] private LoadingUI loadingUI;
    [SerializeField] private Button pauseBtn;
    [SerializeField] private Button speedBtn;

    private CompositeDisposable disposable = new CompositeDisposable();
    private float timeLeft;

    public async UniTask StartLoadingUI()
    {
        loadingUI.SetActive(true);
        await loadingUI.PlayLoadingUIAsync();
    }
    public void EndLoadingUI()
    {
        loadingUI.SetActive(false);

    }
    private void Awake()
    {
        speedBtn.onClick.AddListener(() =>
        {
            Time.timeScale = UserData.Instance.NextGameSpeed();
            UpdateUI();
        });
    }
    public void SetData(long endUnixTime, CancellationTokenSource _cts)
    {
        timeLeft = endUnixTime;
        UniTask.Create(async () =>
        {
            while (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                await UniTask.Yield(cancellationToken: _cts.Token);
            }
            tileLeftText.SetText("00:00");
            MGameManager.Instance.LoseStage();
        });
        UniTask.Create(async () =>
        {
            while (timeLeft > 0)
            {
                var timeSpan = TimeSpan.FromSeconds(timeLeft);
                tileLeftText.SetText(timeSpan.ToString(@"mm\:ss"));
                await UniTask.WaitForSeconds(1f, cancellationToken: _cts.Token);
            }
        });
        disposable.Clear();
        // Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
        //.Select(x => timeLeft -= Time.deltaTime)
        //.TakeWhile(s => s > 0)
        //.Subscribe(timeLeft =>
        //{
        //    var timeSpan = TimeSpan.FromSeconds(timeLeft);
        //    tileLeftText.SetText(timeSpan.ToString(@"mm\:ss"));
        //},
        //() =>
        //{
        //    tileLeftText.SetText("00:00");
        //    MGameManager.Instance.LoseStage();
        //     // To Do : Result
        //}).AddTo(_cts.Token);

        UserData.Instance.AcquireSoul.Subscribe(_soul =>
        {
            acquireSoulText.SetText(_soul.ToString());
        }).AddTo(disposable);
        UpdateUI();
    }

    public void UpdateUI()
    {
        speedIconX1.SetActive(UserData.Instance.GameSpeed == 1);
        speedIconX2.SetActive(UserData.Instance.GameSpeed == 2);
        speedIconX4.SetActive(UserData.Instance.GameSpeed == 4);
    }

    private void OnDisable()
    {
        disposable.Clear();
    }
}

