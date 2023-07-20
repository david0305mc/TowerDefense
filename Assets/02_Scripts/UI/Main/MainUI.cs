using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Button testBtn;
    [SerializeField] private Button shopBtn;

    private void Start()
    {
        UpdateUI();
    }
    private void Awake()
    {
        testBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.SpawnCharacter(3, GroundManager.Instance.GetRandomFreePosition());
        });

        shopBtn.onClick.AddListener(() =>
        {
            UserData.Instance.LocalData.Gold++;
            UserData.Instance.LocalData.TestDic.Add(UserData.Instance.LocalData.Gold, UnityEngine.Random.Range(0, 10));
            UserData.Instance.SaveLocalData();
            PopupManager.Instance.Show<ShopPopup>();
            UpdateUI();
        });
    }

    private void UpdateUI()
    {
        goldText.text = UserData.Instance.LocalData.Gold.ToString();
    }

}
