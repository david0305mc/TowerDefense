using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainBottomTabGroup : MonoBehaviour
{
    [SerializeField] private List<UIMainBottomTab> bottomTabList;

    public void InitTabGroup(int _defaultIndex, System.Action<int> _tabAction)
    {
        void SelectTab(int _selectIndex)
        {
            for (int i = 0; i < bottomTabList.Count; i++)
            {
                if (i == _selectIndex)
                {
                    bottomTabList[i].EnableTab();
                }
                else
                {
                    bottomTabList[i].DisableTab();
                }
            }
        }

        for (int i = 0; i < bottomTabList.Count; i++)
        {
            bottomTabList[i].SetTabData(i, (_index) =>
            {
                SelectTab(_index);
                _tabAction?.Invoke(_index);
            });
        }
        SelectTab(_defaultIndex);
    }
}
