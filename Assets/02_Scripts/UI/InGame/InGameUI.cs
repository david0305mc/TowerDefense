using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private Transform soulTargetTR;
    public Transform SoulTarget => soulTargetTR;
    [SerializeField] private TextMeshProUGUI tileLeftText;
    [SerializeField] private TextMeshProUGUI acquireSoulText;
    [SerializeField] private TextMeshProUGUI devilTalkText;
    [SerializeField] private Animator devilTalkAnimator;
    [SerializeField] private GameObject devilTalkObject;
    [SerializeField] private GameObject speedIconX1;
    [SerializeField] private GameObject speedIconX2;
    [SerializeField] private GameObject speedIconX4;
    [SerializeField] private LoadingUI loadingUI;
    [SerializeField] private Button pauseBtn;
    [SerializeField] private Button speedBtn;

    private double timeLeft;
    private CompositeDisposable disposable = new CompositeDisposable();
    private CancellationTokenSource devilTaklCts;
    private UniTask showDevilTalkTask;
    private long lastShowDevilTalkTime;
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
        timeLeft = 0;
        speedBtn.onClick.AddListener(() =>
        {
            Time.timeScale = UserData.Instance.NextGameSpeed();
            UpdateUI();
        });
        pauseBtn.onClick.AddListener(() =>
        {
            var popup = PopupManager.Instance.Show<PausePopup>();
            popup.SetData(
                () =>
                {
                    if (UserData.Instance.LocalData.Stamina.Value >= ConfigTable.Instance.StageStartCost)
                    {
                        popup.Hide();
                        MGameManager.Instance.RestartStage();
                    }
                    else
                    {
                        PopupManager.Instance.ShowSystemOneBtnPopup("Not enough Stamina", "OK");
                    }
                },
                () =>
                {
                    popup.Hide();
                    MGameManager.Instance.ExitStage();
                });
        });
    }

    private void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
        }
    }
    public void SetData(long endUnixTime, CancellationTokenSource _cts)
    {
        timeLeft = endUnixTime - GameTime.Get();
        UniTask.Create(async () =>
        {
            while (timeLeft > 0)
            {
                var timeSpan = TimeSpan.FromSeconds(timeLeft);
                tileLeftText.SetText(timeSpan.ToString(@"mm\:ss"));
                await UniTask.Yield(_cts.Token);
            }
            tileLeftText.SetText("00:00");
            if (UserData.Instance.IsWaveStage)
            {
                MGameManager.Instance.WinStage();
            }
            else
            {
                MGameManager.Instance.LoseStage();
            }
        });
        disposable?.Clear();

        UserData.Instance.AcquireSoul.Subscribe(_soul =>
        {
            acquireSoulText.SetText(_soul.ToString());
        }).AddTo(disposable);

        devilTalkAnimator.SetTrigger("idle");
        devilTalkObject.SetActive(false);
        lastShowDevilTalkTime = -1;
        UpdateUI();
    }

    public void ShowDevilTalk(int _sayID = -1)
    {
        if (lastShowDevilTalkTime + ConfigTable.Instance.ShowDevilSayTextCoolTime < GameTime.Get())
        {
            lastShowDevilTalkTime = GameTime.Get();
            ShowDevilTalkAsync(_sayID).Forget();
        }
    }

    private async UniTask ShowDevilTalkAsync(int _sayID)
    {
        if (devilTaklCts != null)
        {
            devilTaklCts.Cancel();
        }
        devilTaklCts = new CancellationTokenSource();

        DataManager.DevilSay devilSayInfo;
        if (_sayID == -1)
        {
            devilSayInfo = DataManager.Instance.GetRandomDevilSay();
        }
        else
        {
            devilSayInfo = DataManager.Instance.GetDevilSayData(_sayID);
        }
        
        string animString = "Idle";
        switch (devilSayInfo.anim)
        {
            case 1:
                animString = "Happy";
                break;
            case 2:
                animString = "Angry";
                break;
        }

        devilTalkAnimator.SetTrigger(animString);
        devilTalkText.SetText(LocalizeManager.Instance.GetLocalString(devilSayInfo.saytext));
        devilTalkObject.SetActive(true);
        await UniTask.WaitForSeconds(devilSayInfo.showtime, cancellationToken: devilTaklCts.Token);
        devilTalkAnimator.SetTrigger("Idle");
        devilTalkObject.SetActive(false);
        devilTalkText.SetText(string.Empty);
    }

    public void UpdateUI()
    {
        speedIconX1.SetActive(UserData.Instance.GameSpeed == 1);
        speedIconX2.SetActive(UserData.Instance.GameSpeed == 2);
        speedIconX4.SetActive(UserData.Instance.GameSpeed == 4);
    }

    private void OnEnable()
    {
        if (UserData.Instance.IsOnTutorial())
        {
            pauseBtn.SetActive(false);
            speedBtn.SetActive(false);
        }
        else
        {
            pauseBtn.SetActive(true);
            speedBtn.SetActive(UserData.Instance.LocalData.Level.Value > 2);
        }
    }

    private void OnDisable()
    {
        disposable.Clear();
        devilTaklCts?.Cancel();
        timeLeft = 0;
    }

    private void OnDestroy()
    {
        disposable.Clear();
        disposable.Dispose();
        devilTaklCts?.Cancel();
        devilTaklCts?.Dispose();
    }
}

