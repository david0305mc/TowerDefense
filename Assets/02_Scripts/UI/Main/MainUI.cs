using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
public class MainUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Button testBtn;
    [SerializeField] private Button loadEnemyBtn;
    [SerializeField] private Button shopBtn;

    private void Awake()
    {
        testBtn.onClick.AddListener(() =>
        {
            UserData.Instance.SaveEnemyData();
        });

        loadEnemyBtn.onClick.AddListener(() =>
        {
            var enemyData = UserData.Instance.LoadInBuildData();

            foreach (var item in enemyData.BaseObjDic)
            {
                GameManager.Instance.SpawnBaseObjEvent(item.Value.TID, item.Value.X, item.Value.Y, true);
            }
        });
        UserData.Instance.LocalData.Gold.Subscribe(v =>
        {
            goldText.SetText(v.ToString());
        }).AddTo(gameObject);

        shopBtn.onClick.AddListener(() =>
        {
            UserData.Instance.LocalData.Gold.Value++;
            UserData.Instance.SaveLocalData();
            PopupManager.Instance.Show<ShopPopup>();
        });
    }
}
