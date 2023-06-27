#pragma warning disable 114
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public partial class DataManager {
	public partial class ItemLevel {
		public int id;
		public int groupid;
		public string fgroupname;
		public string name;
		public string desc;
		public int level;
		public int cooltime;
		public int get_value;
		public int cost;
		public string iconpath;
	};
	public ItemLevel[] ItemlevelArray { get; private set; }
	public Dictionary<int, ItemLevel> ItemlevelDic { get; private set; }
	public void BindItemLevelData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(ItemlevelArray)).SetValue(this, deserializaedData, null);
		ItemlevelDic = ItemlevelArray.ToDictionary(i => i.id);
	}
	public ItemLevel GetItemLevelData(int _id){
		if (ItemlevelDic.TryGetValue(_id, out ItemLevel value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class MergeBoard {
		public int id;
		public int col;
		public int row;
		public int itemid;
		public ITEM_STATUS type;
	};
	public MergeBoard[] MergeboardArray { get; private set; }
	public Dictionary<int, MergeBoard> MergeboardDic { get; private set; }
	public void BindMergeBoardData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(MergeboardArray)).SetValue(this, deserializaedData, null);
		MergeboardDic = MergeboardArray.ToDictionary(i => i.id);
	}
	public MergeBoard GetMergeBoardData(int _id){
		if (MergeboardDic.TryGetValue(_id, out MergeBoard value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class BoxTable {
		public int id;
		public int itemid;
		public int rewardid;
		public int weight;
		public string fname;
	};
	public BoxTable[] BoxtableArray { get; private set; }
	public Dictionary<int, BoxTable> BoxtableDic { get; private set; }
	public void BindBoxTableData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(BoxtableArray)).SetValue(this, deserializaedData, null);
		BoxtableDic = BoxtableArray.ToDictionary(i => i.id);
	}
	public BoxTable GetBoxTableData(int _id){
		if (BoxtableDic.TryGetValue(_id, out BoxTable value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class BoxTimeTable {
		public int id;
		public int cooltime;
		public int chargecount;
		public int chargetime;
	};
	public BoxTimeTable[] BoxtimetableArray { get; private set; }
	public Dictionary<int, BoxTimeTable> BoxtimetableDic { get; private set; }
	public void BindBoxTimeTableData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(BoxtimetableArray)).SetValue(this, deserializaedData, null);
		BoxtimetableDic = BoxtimetableArray.ToDictionary(i => i.id);
	}
	public BoxTimeTable GetBoxTimeTableData(int _id){
		if (BoxtimetableDic.TryGetValue(_id, out BoxTimeTable value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class ItemGroup {
		public int id;
		public ITEM_TYPE type;
	};
	public ItemGroup[] ItemgroupArray { get; private set; }
	public Dictionary<int, ItemGroup> ItemgroupDic { get; private set; }
	public void BindItemGroupData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(ItemgroupArray)).SetValue(this, deserializaedData, null);
		ItemgroupDic = ItemgroupArray.ToDictionary(i => i.id);
	}
	public ItemGroup GetItemGroupData(int _id){
		if (ItemgroupDic.TryGetValue(_id, out ItemGroup value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class ScenarioMissionTable {
		public int id;
		public string missionname;
		public int missionitemid1;
		public int missionitemcount1;
		public int missionitemid2;
		public int missionitemcount2;
		public int rewarditemid1;
		public int rewarditemcount1;
		public int rewarditemid2;
		public int rewarditemcount2;
		public SLOT_ANIM_TYPE slotanimtype;
		public int slotid;
		public int dialoggroupid;
	};
	public ScenarioMissionTable[] ScenariomissiontableArray { get; private set; }
	public Dictionary<int, ScenarioMissionTable> ScenariomissiontableDic { get; private set; }
	public void BindScenarioMissionTableData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(ScenariomissiontableArray)).SetValue(this, deserializaedData, null);
		ScenariomissiontableDic = ScenariomissiontableArray.ToDictionary(i => i.id);
	}
	public ScenarioMissionTable GetScenarioMissionTableData(int _id){
		if (ScenariomissiontableDic.TryGetValue(_id, out ScenarioMissionTable value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class ScenarioSlotTable {
		public int id;
		public string fname;
	};
	public ScenarioSlotTable[] ScenarioslottableArray { get; private set; }
	public Dictionary<int, ScenarioSlotTable> ScenarioslottableDic { get; private set; }
	public void BindScenarioSlotTableData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(ScenarioslottableArray)).SetValue(this, deserializaedData, null);
		ScenarioslottableDic = ScenarioslottableArray.ToDictionary(i => i.id);
	}
	public ScenarioSlotTable GetScenarioSlotTableData(int _id){
		if (ScenarioslottableDic.TryGetValue(_id, out ScenarioSlotTable value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class ScenarioTable {
		public int id;
		public string scenarioname;
		public int missioncount;
		public int unlockscenarioid;
		public string missionlist;
	};
	public ScenarioTable[] ScenariotableArray { get; private set; }
	public Dictionary<int, ScenarioTable> ScenariotableDic { get; private set; }
	public void BindScenarioTableData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(ScenariotableArray)).SetValue(this, deserializaedData, null);
		ScenariotableDic = ScenariotableArray.ToDictionary(i => i.id);
	}
	public ScenarioTable GetScenarioTableData(int _id){
		if (ScenariotableDic.TryGetValue(_id, out ScenarioTable value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class ScenarioDialogTable {
		public int id;
		public int dialoggroupid;
		public string leftimg;
		public string rightimg;
		public string script;
	};
	public ScenarioDialogTable[] ScenariodialogtableArray { get; private set; }
	public Dictionary<int, ScenarioDialogTable> ScenariodialogtableDic { get; private set; }
	public void BindScenarioDialogTableData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(ScenariodialogtableArray)).SetValue(this, deserializaedData, null);
		ScenariodialogtableDic = ScenariodialogtableArray.ToDictionary(i => i.id);
	}
	public ScenarioDialogTable GetScenarioDialogTableData(int _id){
		if (ScenariodialogtableDic.TryGetValue(_id, out ScenarioDialogTable value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
};
