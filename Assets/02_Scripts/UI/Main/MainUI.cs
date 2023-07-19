using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{

    [SerializeField] private Button testBtn;
    [SerializeField] private Button shopBtn;

    private void Awake()
    {
        testBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.SpawnCharacter(3, GroundManager.Instance.GetRandomFreePosition());
        });

        shopBtn.onClick.AddListener(() =>
        {
            PopupManager.Instance.Show<ShopPopup>();
        });
    }

}
