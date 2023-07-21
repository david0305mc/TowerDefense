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
    [SerializeField] private Button shopBtn;

    private void Awake()
    {
        testBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.SpawnBaseObjEvent(3, (int)GroundManager.Instance.GetRandomFreePosition().x, (int)GroundManager.Instance.GetRandomFreePosition().y, false);
            //GameManager.Instance.SpawnCharacter(3, GroundManager.Instance.GetRandomFreePosition());
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
