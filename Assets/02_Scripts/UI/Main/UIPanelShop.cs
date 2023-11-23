using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;

public class UIPanelShop : MonoBehaviour
{
    [SerializeField] private Button summonHero_10_Button;
    [SerializeField] private Button summonHero_1_Button;
    [SerializeField] private Button freeSummon_Button;
    [SerializeField] private Button buyStamina_10_Button;
    [SerializeField] private Button buyStamina_50_Button;
    [SerializeField] private Button buyStamina_100_Button;
    [SerializeField] private TextMeshProUGUI freeGachaText;
    [SerializeField] private ScrollRect scrollRect;


    private Image freeSummonBtnImage;
    private CancellationTokenSource cts;
    private Color freeSummonOrgColor;

    private void Awake()
    {
        freeSummonBtnImage = freeSummon_Button.GetComponent<Image>();
        freeSummonOrgColor = freeSummonBtnImage.color;

        summonHero_10_Button.onClick.AddListener(() =>
        {
            SummonHero(10, 1000);
        });
        summonHero_1_Button.onClick.AddListener(() =>
        {
            SummonHero(1, 100);
        });
        buyStamina_10_Button.onClick.AddListener(() =>
        {
            BuyStamina(10, 1000);
        });
        buyStamina_50_Button.onClick.AddListener(() =>
        {
            BuyStamina(50, 4500);
        });
        buyStamina_100_Button.onClick.AddListener(() =>
        {
            BuyStamina(100, 8000);
        });
        freeSummon_Button.onClick.AddListener(() =>
        {
            SummonFreeGacha();
        });
    }
    private void OnEnable()
    {
        scrollRect.verticalNormalizedPosition = 1f;

        if (UserData.Instance.LocalData.FreeGachaRewardableTime <= GameTime.Get())
        {
            EnableFreeGacha();
        }
        else
        {
            WaitForReward();
        }
    }
    private void OnDisable()
    {
        cts?.Cancel();
        cts = null;
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    private void EnableFreeGacha()
    {
        freeGachaText.SetText(LocalizeManager.Instance.GetLocalString("gacha_dragon_btn"));
        freeSummon_Button.enabled = true;
        freeSummonBtnImage.color = freeSummonOrgColor;
    }

    private void DisableFreeGacha()
    {
        freeSummonBtnImage.color = new Color(0, 0, 0, 100 / (float)255);
        freeSummon_Button.enabled = false;
    }

    public void WaitForReward()
    {
        DisableFreeGacha();
        if (cts != null)
        {
            cts?.Cancel();
        }
        cts = new CancellationTokenSource();

        UniTask.Create(async () =>
        {
            while (UserData.Instance.LocalData.FreeGachaRewardableTime > GameTime.Get())
            {
                var seconds = UserData.Instance.LocalData.FreeGachaRewardableTime - GameTime.Get();
                freeGachaText.SetText(System.TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss"));
                await UniTask.WaitForSeconds(0.1f, cancellationToken: cts.Token);
            }
            EnableFreeGacha();
        });
    }

    private void SummonFreeGacha()
    {
        SummonHero(3, 0);
        UserData.Instance.LocalData.FreeGachaRewardableTime = GameTime.Get() + ConfigTable.Instance.FreeGachaCooltime;
        WaitForReward();
    }
    private void SummonHero(int _count, int _goldCost)
    {
        if (UserData.Instance.LocalData.Gold.Value < _goldCost)
        {
            PopupManager.Instance.ShowSystemOneBtnPopup(LocalizeManager.Instance.GetLocalString("notEnoughGold"), "OK");
            return;
        }
        MGameManager.Instance.SummonHero(_count, _goldCost);
    }

    private void BuyStamina(int _count, int _goldCost)
    {
        if (UserData.Instance.LocalData.Gold.Value < _goldCost)
        {
            PopupManager.Instance.ShowSystemOneBtnPopup(LocalizeManager.Instance.GetLocalString("notEnoughGold"), "OK");
            return;
        }
        MGameManager.Instance.BuyStamina(_count, _goldCost);
    }

}
