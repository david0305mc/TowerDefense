using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public partial class DataManager
{
    public partial class ItemLevel
    {
        public ItemGroup ItemGroupRef { get; private set; }
        public void InitSubData()
        {
             ItemGroupRef = Instance.GetItemGroupData(groupid);
        }
    };

	public partial class ScenarioTable
	{
        public List<int> missionList { get; set; } 
        public void InitSubData()
        {
            var array = missionlist.Split(',');
            missionList = new List<int>();
            foreach (var item in array)
            {
                missionList.Add(int.Parse(item));
            }
        }

        public bool IsComplete(List<int> clearList)
        {
            return missionList.All(item => clearList.Contains(item));
        }
    };

}
