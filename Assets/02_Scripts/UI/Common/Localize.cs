using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Localize : MonoBehaviour
{
    [SerializeField] private string key;
    private TextMeshProUGUI uiText;

    private void Start()
    {
        uiText = GetComponent<TextMeshProUGUI>();
        if (uiText != null)
        {
            UpdateLocalization();
        }
    }

    private void UpdateLocalization()
    {
        string localString = LocalizeManager.Instance.GetLocalString(key);
        if (string.IsNullOrEmpty(localString))
        {
            Debug.LogError($"there is no key {key}");
            uiText.SetText(key);
        }
        else
        {
            uiText.SetText(localString);
        }
    }

    private void OnLocalize()
    {
        UpdateLocalization();
    }
}
