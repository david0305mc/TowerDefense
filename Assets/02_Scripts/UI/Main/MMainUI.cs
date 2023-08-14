using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MMainUI : MonoBehaviour
{
    [SerializeField] private Button test01Btn;
    private void Awake()
    {
        test01Btn.onClick.AddListener(() =>
        {
            MGameManager.Instance.AddHero();
        });
    }
}
