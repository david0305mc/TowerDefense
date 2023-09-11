using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMainBottomTab : MonoBehaviour
{
    [SerializeField] private List<GameObject> onObject;
    [SerializeField] private Button tabButton;

    public void SetTabData(int _index, System.Action<int> _tabAction)
    {
        tabButton.onClick.RemoveAllListeners();
        tabButton.onClick.AddListener(() =>
        {
            _tabAction?.Invoke(_index);
        });
    }

    public void EnableTab()
    {
        foreach (var item in onObject)
        {
            item.SetActive(true);
        }
    }

    public void DisableTab()
    {
        foreach (var item in onObject)
        {
            item.SetActive(false);
        }
    }
}
