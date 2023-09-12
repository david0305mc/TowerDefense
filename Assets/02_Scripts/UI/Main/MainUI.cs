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
    [SerializeField] private Button testBtn;
    [SerializeField] private Button loadEnemyBtn;
    [SerializeField] private Button shopBtn;
    [SerializeField] private UIMainBottomTabGroup tabGruop;
    [SerializeField] private GameObject stageInfoPanel;
         

    private void Awake()
    {
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
        stageInfoPanel.SetActive(false);
    }

    public void ShowStageInfo(bool _value)
    {
        stageInfoPanel.SetActive(_value);
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
                    //MGameManager.Instance.SpawnStage(UserData.Instance.CurrStage);
                }
                break;
            case BottomTab.Pvp:
                {
                    //MGameManager.Instance.AddHero(1);
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
