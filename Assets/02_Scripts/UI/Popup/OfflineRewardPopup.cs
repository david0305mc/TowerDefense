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
        var seconds = GameTime.Get() - UserData.Instance.LocalData.LastLoginTime;
        long maxSeconds = (long)System.TimeSpan.FromHours(1).TotalSeconds;
        string param01 = System.TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss");
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
        long goldSeconds = seconds > maxSeconds ? maxSeconds : seconds;
        gauge.value = goldSeconds / (float)maxSeconds;

        int goldproductterm = DataManager.Instance.GetStageInfoData(1).goldproductterm;
        int goldAmt = Mathf.FloorToInt(goldSeconds / (float)goldproductterm) * goldAcc;
        goldText.SetText(goldAmt.GetCommaString());

        receiveBtn.onClick.RemoveAllListeners();
        receiveBtn.onClick.AddListener(() =>
        {
            _action?.Invoke(goldAmt);
            Hide();
        });

    }
}
