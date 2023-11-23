using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OfflineRewardPopup : PopupBase
{
    [SerializeField] private MButton receiveBtn;
    [SerializeField] private TextMeshProUGUI timeDescText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Slider gauge;

    protected override void Awake()
    {
        base.Awake(); 
    }

    public void SetData(System.Action<int> _action)
    {
        var seconds = UserData.Instance.OfflineTimeSeconds;
        long maxSeconds = ConfigTable.Instance.OfflineRewardMaxTime;
        long goldSeconds = seconds > maxSeconds ? maxSeconds : seconds;

        string param01 = System.TimeSpan.FromSeconds(goldSeconds).ToString(@"hh\:mm\:ss");
        string param02 = System.TimeSpan.FromSeconds(maxSeconds).ToString(@"hh\:mm\:ss");
        timeDescText.SetText($"{param01} / {param02}");

        int goldAcc = 0;
        foreach (var item in DataManager.Instance.StageinfoArray)
        {
            if (UserData.Instance.LocalData.StageClearDic.ContainsKey(item.id))
            {
                goldAcc += item.goldproductamount;
            }
        }
        gauge.value = goldSeconds / (float)maxSeconds;
        if (goldSeconds == 0)
        {
            Debug.LogError($"seconds {goldSeconds} maxSeconds {maxSeconds}");
        }

        int d = ConfigTable.Instance.OfflineRewardRate;
        int goldproductterm = DataManager.Instance.GetStageInfoData(1).goldproductterm;
        int goldAmt = Mathf.FloorToInt(goldSeconds / (float)goldproductterm) * goldAcc * d / 100;
        goldText.SetText(goldAmt.GetCommaString());

        receiveBtn.onClick.RemoveAllListeners();
        receiveBtn.onClick.AddListener(() =>
        {
            _action?.Invoke(goldAmt);
            Hide();
        });

    }
}
