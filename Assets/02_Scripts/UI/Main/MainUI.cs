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
         

    private void Awake()
    {
        tabGruop.InitTabGroup(2, (_index) =>
        {
            OnBottomTabClicked(_index);
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
    }
    private void OnBottomTabClicked(int _index)
    {
        switch ((BottomTab)_index)
        {
            case BottomTab.Worldmap:

                break;
        }
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
