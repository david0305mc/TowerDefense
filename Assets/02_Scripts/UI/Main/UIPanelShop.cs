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
            var gachaList = DataManager.Instance.GetGachaResultList(5);
            foreach (var item in gachaList)
            {
                UserData.Instance.AddHeroData(item);
            }
            var popup = PopupManager.Instance.Show<GachaResultPopup>();
            popup.SetData(gachaList);
        });
    }
    public void InitUI()
    {

    }


}
