using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelShop : MonoBehaviour
{
    [SerializeField] private Button gachaButton;
    private void Awake()
    {
        gachaButton.onClick.AddListener(() =>
        {
            //AddHeroData(2001);
            //AddHeroData(2002);
            //AddHeroData(2003);
            //AddHeroData(2004);
            //AddHeroData(2005);


            //UserData.Instance.AddHeroData

            var popup = PopupManager.Instance.Show<GachaResultPopup>();
            popup.SetData();
        });
    }
    public void InitUI()
    {

    }


}
