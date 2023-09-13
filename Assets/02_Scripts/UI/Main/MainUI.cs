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

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Button addTankerBtn;
    [SerializeField] private Button addArcherBtn;
    [SerializeField] private Button WinBtn;
    [SerializeField] private UIMainBottomTabGroup tabGruop;
    
    [SerializeField] private UIPanelStageInfo stageInfoPanel;
    [SerializeField] private UIPanelUnit unitPanel;

    private void Awake()
    {
        addTankerBtn.onClick.AddListener(() =>
        {
            MGameManager.Instance.AddHero(2001);
        });
        addArcherBtn.onClick.AddListener(() =>
        {
            MGameManager.Instance.AddHero(2002);
        });
        WinBtn.onClick.AddListener(() =>
        {
            var popup = PopupManager.Instance.Show<UnitInfoPopup>(()=> { 
                // Hide

            });
            
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
                    MCameraManager.Instance.SetZoomAndSize(2, 7, -2, 2, -2, 2);
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
