using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Localize : MonoBehaviour
{
    [SerializeField] private string key;
    private TextMeshProUGUI uiText;
         
    private void Awake()
    {
        uiText = GetComponent<TextMeshProUGUI>();
        if (uiText != null)
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
    }
}
