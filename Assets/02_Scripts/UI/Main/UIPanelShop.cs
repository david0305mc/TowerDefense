using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelShop : MonoBehaviour
{
    [SerializeField] private Button gachaButton;
    [SerializeField] private ScrollRect scrollRect;
    private void Awake()
    {
        gachaButton.onClick.AddListener(() =>
        {
            var gachaList = DataManager.Instance.GenerateGachaResultList(10);
            foreach (var item in gachaList)
            {
                var gachaInfo = DataManager.Instance.GetGachaListData(item);
                MGameManager.Instance.AddHero(gachaInfo.unitid, gachaInfo.count);
            }
            var popup = PopupManager.Instance.Show<GachaResultPopup>();
            popup.SetData(gachaList);
        });
    }
    private void OnEnable()
    {
        scrollRect.verticalNormalizedPosition = 1f;
    }


}
