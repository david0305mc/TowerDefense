using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class DataManager 
{
    public void MakeClientDT()
    {
		foreach (var item in ScenariotableArray)
		{
			item.InitSubData();
		}

		foreach (var item in ItemlevelDic)
		{
			item.Value.InitSubData();
		}
    }

	#region ItemLevel 
	public ItemLevel GetItemLevelData(int _groupID, int _level)
	{
		var item = (from data in ItemlevelDic
					where data.Value.groupid == _groupID && data.Value.level == _level
					select data.Value).FirstOrDefault();

		if (item == default)
		{
			Debug.LogError($"_groupID {_groupID } _level {_level}");
		}
		return item;
	}
	#endregion


	#region BoxTable
	public BoxTable GenerateItemFromBox(int _boxID)
	{
		var rewardLists = System.Array.FindAll(BoxtableArray, item => item.itemid == _boxID).OrderByDescending(c => c.weight).ToList();
		int sum = rewardLists.Sum(item => item.weight);
		int randNum = Random.Range(0, sum);
		foreach (var item in rewardLists)
		{
			randNum -= item.weight;
			if (randNum <= 0)
			{
				return item;
			}
		}
		return rewardLists.Last();
	}
	#endregion

	#region ScenarioDialogTable
	public List<ScenarioDialogTable> GetDialogList(int _groupID)
	{
		var ret = (from data in ScenariodialogtableDic
				   where data.Value.dialoggroupid == _groupID
				   orderby data.Key ascending
				   select data.Value).ToList();
		return ret;
	}
	#endregion

	#region ScenarioMissionTable
	public ScenarioMissionTable GetScenarioMissionTableBySlotID(int _slotID)
	{
		var ret = (from data in ScenariomissiontableDic
				   where data.Value.slotid == _slotID
				   select data.Value).FirstOrDefault();
		return ret;
	}
    #endregion
}
