using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System.Threading;

public class MainUI : MonoBehaviour
{
    enum BottomTab
    {
        Shop,
        Arrangement,
        Worldmap,
        Pvp,
        Event,
    }

    [SerializeField] private TextMeshProUGUI soulText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private Button test01Btn;
    [SerializeField] private Button WinBtn;
    [SerializeField] private UIMainBottomTabGroup tabGruop;

    [SerializeField] private UIPanelStageInfo stageInfoPanel;
    [SerializeField] private UIPanelUnitSelect unitSelectPanel;
    [SerializeField] private GameObject worldUI;
    [SerializeField] private GameObject ingameUI;
    [SerializeField] private UIStage uiStage;

    public UIStage GetUIStage => uiStage;

    private MHeroObj heroObjTest;
    private void Awake()
    {
        UserData.Instance.LocalData.Soul.Subscribe(_value =>
        {
            soulText.SetText(_value.ToString());
        }).AddTo(gameObject);

        UserData.Instance.LocalData.Stamina.Subscribe(_value =>
        {
            staminaText.SetText(_value.ToString());
        }).AddTo(gameObject);

        UserData.Instance.LocalData.Exp.Subscribe(_value =>
        {
            expText.SetText(_value.ToString());
        }).AddTo(gameObject);

        test01Btn.onClick.AddListener(() =>
        {
            MGameManager.Instance.LoseStage();
        });

        WinBtn.onClick.AddListener(() =>
        {
            MGameManager.Instance.WinStage();
        });
        HideStageInfo();
    }

    public void SetWorldUI()
    {
        worldUI.SetActive(true);
        ingameUI.SetActive(false);
    }
    public void SetIngameUI()
    {
        worldUI.SetActive(false);
        ingameUI.SetActive(true);
    }

    public void SetStageUI(CancellationTokenSource _cts)
    {
        var stageInfo = DataManager.Instance.GetStageInfoData(UserData.Instance.CurrStage);
        uiStage.SetData(GameTime.Get() + stageInfo.stagecleartime, _cts);
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
        if (tabGruop.CurrTabIndex == (int)BottomTab.Arrangement)
        {
            HideArrangementUI();
        }

        switch ((BottomTab)_index)
        {
            case BottomTab.Worldmap:
                {
                    MCameraManager.Instance.SetZoomAndSize(2, 7, -2, 9, -2, 6);
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
                    
                }
                break;
        }
    }
    private void SelectTab(BottomTab _tab)
    {
        tabGruop.SelectTab((int)_tab);
    }

    public void OnClicShopBtn()
    { 
    
    }
    public void OnClicArrangeBtn()
    {

    }
    public void OnClicWorldMapBtn()
    {

    }
    public void OnClicPvpBtn()
    {

    }
    public void OnClicEventBtn()
    {

    }
}
