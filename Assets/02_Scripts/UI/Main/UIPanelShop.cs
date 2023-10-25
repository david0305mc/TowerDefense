using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelShop : MonoBehaviour
{
    [SerializeField] private Button summonHero_10_Button;
    [SerializeField] private Button summonHero_3_Button;
    [SerializeField] private Button buyStamina_10_Button;
    [SerializeField] private Button buyStamina_50_Button;
    [SerializeField] private Button buyStamina_100_Button;
    [SerializeField] private ScrollRect scrollRect;
    private void Awake()
    {
        summonHero_10_Button.onClick.AddListener(() =>
        {
            SummonHero(10, 1000);
        });
        summonHero_3_Button.onClick.AddListener(() =>
        {
            SummonHero(3, 300);
        });
        buyStamina_10_Button.onClick.AddListener(() =>
        {
            BuyStamina(10, 10);
        });
        buyStamina_50_Button.onClick.AddListener(() =>
        {
            BuyStamina(50, 50);
        });
        buyStamina_100_Button.onClick.AddListener(() =>
        {
            BuyStamina(100, 100);
        });
    }
    private void OnEnable()
    {
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private void SummonHero(int _count, int _goldCost)
    {
        if (UserData.Instance.LocalData.Gold.Value < _goldCost)
        {
            PopupManager.Instance.ShowSystemOneBtnPopup("Not enough Gold", "OK");
            return;
        }
        MGameManager.Instance.SummonHero(_count, _goldCost);
    }

    private void BuyStamina(int _count, int _goldCost)
    {
        if (UserData.Instance.LocalData.Gold.Value < _goldCost)
        {
            PopupManager.Instance.ShowSystemOneBtnPopup("Not enough Gold", "OK");
            return;
        }
        MGameManager.Instance.BuyStamina(_count, _goldCost);
    }

}
