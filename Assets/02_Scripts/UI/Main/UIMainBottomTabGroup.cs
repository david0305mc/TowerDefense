using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainBottomTabGroup : MonoBehaviour
{
    [SerializeField] private List<UIMainBottomTab> bottomTabList;

    private System.Action<int> tabAction;
    private int currTabIndex;
    public int CurrTabIndex => currTabIndex;
    private void Awake()
    {
        currTabIndex = -1;
    }
    public void SelectTab(int _index)
    {
        for (int i = 0; i < bottomTabList.Count; i++)
        {
            if (i == _index)
            {
                bottomTabList[i].EnableTab();
            }
            else
            {
                bottomTabList[i].DisableTab();
            }
        }
        tabAction?.Invoke(_index);
        currTabIndex = _index;
    }

    public void InitTabGroup(int _defaultIndex, System.Action<int> _tabAction)
    {
        tabAction = _tabAction;
        for (int i = 0; i < bottomTabList.Count; i++)
        {
            bottomTabList[i].SetTabData(i, (_index) =>
            {
                SelectTab(_index);
            });
        }
        SelectTab(_defaultIndex);
    }
}
