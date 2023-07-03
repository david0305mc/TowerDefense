using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMain : MonoBehaviour
{
    [SerializeField] private Button buildTestBtn;
    private void Awake()
    {
        buildTestBtn.onClick.AddListener(() => {
            AddBuildingObj();
        });
    }

    private void AddBuildingObj()
    {
        GameManager.Instance.SpawnItem();
    }
}
