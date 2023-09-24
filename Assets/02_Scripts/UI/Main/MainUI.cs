using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
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
    [SerializeField] private Button addTankerBtn;
    [SerializeField] private Button addArcherBtn;
    [SerializeField] private Button WinBtn;
    [SerializeField] private UIMainBottomTabGroup tabGruop;
    
    [SerializeField] private UIPanelStageInfo stageInfoPanel;
    [SerializeField] private UIPanelUnitSelect unitSelectPanel;

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

        addTankerBtn.onClick.AddListener(() =>
        {
            //MGameManager.Instance.AddHero(2001);
        });
        addArcherBtn.onClick.AddListener(() =>
        {
            //MGameManager.Instance.AddHero(2002);
        });
        WinBtn.onClick.AddListener(() =>
        {
            //var popup = PopupManager.Instance.Show<UnitInfoPopup>(()=> { 
            //    // Hide

            //});
            MGameManager.Instance.WinStage();

        });
        //testBtn.onClick.AddListener(() =>
        //{
        //    UserData.Instance.SaveEnemyData();
        //});

        //loadEnemyBtn.onClick.AddListener(() =>
        //{

        //});
        //UserData.Instance.LocalData.Gold.Subscribe(v =>
        //{
        //    goldText.SetText(v.ToString());
        //}).AddTo(gameObject);

        //shopBtn.onClick.AddListener(() =>
        //{
        //    UserData.Instance.LocalData.Gold.Value++;
        //    UserData.Instance.SaveLocalData();
        //    PopupManager.Instance.Show<ShopPopup>();
        //});
        HideStageInfo();
    }

    public void ShowStageInfo(int _stageID, System.Action _startAction)
    {
        stageInfoPanel.gameObject.SetActive(true);
        stageInfoPanel.SetData(_stageID, () => {
            _startAction?.Invoke();
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
