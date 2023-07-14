using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{

    [SerializeField] private Button testBtn;

    private void Awake()
    {
        testBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.SpawnCharacter();
        });
    }

}
