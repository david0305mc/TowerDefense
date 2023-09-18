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
            var popup = PopupManager.Instance.Show<GachaResultPopup>();
            popup.SetData();
        });
    }
    public void InitUI()
    {

    }


}
