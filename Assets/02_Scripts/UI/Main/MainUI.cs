using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;

public class MainUI : MonoBehaviour
{
    public enum BottomTab
    {
        Shop,
        Arrangement,
        Worldmap,
        Pvp,
        Event,
    }

    [SerializeField] private Transform goldTargetTR;
    public Transform GoldTarget => goldTargetTR;

    [SerializeField] private TextMeshProUGUI soulText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI staminaTimerText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private UIMainBottomTabGroup tabGruop;

    [SerializeField] private UIPanelStageInfo stageInfoPanel;
    [SerializeField] private UIPanelUnitSelect unitSelectPanel;

    private CompositeDisposable disposable = new CompositeDisposable();

    private void OnEnable()
    {
        UserData.Instance.LocalData.Soul.Subscribe(_value =>
        {
            soulText.SetText(_value.ToString());
        }).AddTo(disposable);

        UserData.Instance.LocalData.Stamina.Subscribe(_value =>
        {
            staminaText.SetText(_value.ToString());
        }).AddTo(disposable);

        UserData.Instance.LocalData.Exp.Subscribe(_value =>
        {
            expText.SetText(_value.ToString());
        }).AddTo(disposable);

        UserData.Instance.LocalData.Gold.Subscribe(_value =>
        {
            goldText.SetText(_value.ToString());
        }).AddTo(disposable);
    }

    private void OnDisable()
    {
        disposable.Clear();
    }

    private void OnDestroy()
    {
        disposable.Clear();
        disposable.Dispose();
    }

    private void Awake()
    {
        HideStageInfo();
        CheckStaminaTimer().Forget();
        
    }

    private async UniTask CheckStaminaTimer()
    {
        while (true)
        {
            if (ConfigTable.Instance.StaminaMaxCount <= UserData.Instance.LocalData.Stamina.Value)
            {
                staminaTimerText.SetText("Max");
            }
            else
            {
                long timeLeft = UserData.Instance.LocalData.StaminaLastSpawnTime + ConfigTable.Instance.StaminaChargeTime - GameTime.Get();
                if (timeLeft <= 0)
                {
                    TimeSpan span = TimeSpan.FromSeconds(1);
                    staminaTimerText.SetText(span.ToString(@"mm\:ss"));
                }
                else
                {
                    TimeSpan span = TimeSpan.FromSeconds(timeLeft);
                    staminaTimerText.SetText(span.ToString(@"mm\:ss"));
                }
            }
            
            await UniTask.WaitForSeconds(0.1f, cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }

    public void ShowStageInfo(int _stageID, System.Action _startAction)
    {
        stageInfoPanel.gameObject.SetActive(true);
        stageInfoPanel.SetData(_stageID, () => {
            _startAction?.Invoke();
        }, ()=> {
            stageInfoPanel.gameObject.SetActive(false);
        });
    }

    public void HideStageInfo()
    {
        stageInfoPanel.gameObject.SetActive(false);
    }

    public void ShowArrangementUI()
    {
        unitSelectPanel.SetActive(true);
        unitSelectPanel.InitUI();
    }

    public void HideArrangementUI()
    {
        unitSelectPanel.SetActive(false);
    }

    public void InitTabGroup()
    {
        tabGruop.InitTabGroup((int)BottomTab.Worldmap, (_index) =>
        {
            OnBottomTabClicked(_index);
        });
    }
    private void OnBottomTabClicked(int _index)
    {
        switch ((BottomTab)_index)
        {
            case BottomTab.Worldmap:
                {
                    MCameraManager.Instance.SetZoomAndSize(Game.GameConfig.DefaultZoomSize, 2, 5, -2, 9, -2, 6);
                    MGameManager.Instance.FollowToCurrStage();
                }
                break;

            case BottomTab.Arrangement:
                {
                    ShowArrangementUI();  
                }
                break;
            case BottomTab.Event:
                {
                }
                break;
            case BottomTab.Pvp:
                {
                    //UI 에서 시작 버튼 눌러야 시작되게끔 수정 필요
                    //MGameManager.Instance.StartWaveStage();
                }
                break;
        }
    }
    public void SelectTab(BottomTab _tab)
    {
        tabGruop.SelectTab((int)_tab);
    }

    public void ShowUnitInfo(int _index)
    {
        unitSelectPanel.ShowUnitInfoPopup(_index);
    }
}
