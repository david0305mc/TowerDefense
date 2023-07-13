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
        foreach (var item in DataManager.Instance.ObjtableArray)
        {
            Debug.Log($"item {item.name}");
        }
        
        return;
        GameManager.Instance.SpawnCharacter();
    }
}
