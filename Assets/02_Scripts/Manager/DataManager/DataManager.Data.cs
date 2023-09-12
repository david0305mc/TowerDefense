#pragma warning disable 114
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public partial class DataManager {
	public partial class Unitinfo {
		public int id;
		public UNIT_TYPE unit_type;
		public int checkrange;
		public int aggroorder;
		public int defaultgrade;
		public int maxgrade;
		public int unitrarity;
		public string unitname;
		public string thumbnailpath;
		public string prefabname;
	};
	public Unitinfo[] UnitinfoArray { get; private set; }
	public Dictionary<int, Unitinfo> UnitinfoDic { get; private set; }
	public void BindUnitinfoData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(UnitinfoArray)).SetValue(this, deserializaedData, null);
		UnitinfoDic = UnitinfoArray.ToDictionary(i => i.id);
	}
	public Unitinfo GetUnitinfoData(int _id){
		if (UnitinfoDic.TryGetValue(_id, out Unitinfo value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class UnitGradeInfo {
		public int id;
		public int unitid;
		public int grade;
		public int hp;
		public int walkspeed;
		public int attackdmg;
		public int attackrange;
		public int attackcount;
		public int multiattackcount;
		public int attackshortdelay;
		public int attacklongdelay;
		public int projectileid;
		public int splashdmg;
		public int splashrange;
		public string boomeffectprefab;
	};
	public UnitGradeInfo[] UnitgradeinfoArray { get; private set; }
	public Dictionary<int, UnitGradeInfo> UnitgradeinfoDic { get; private set; }
	public void BindUnitGradeInfoData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(UnitgradeinfoArray)).SetValue(this, deserializaedData, null);
		UnitgradeinfoDic = UnitgradeinfoArray.ToDictionary(i => i.id);
	}
	public UnitGradeInfo GetUnitGradeInfoData(int _id){
		if (UnitgradeinfoDic.TryGetValue(_id, out UnitGradeInfo value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class ProjectileInfo {
		public int id;
		public string prefabname;
	};
	public ProjectileInfo[] ProjectileinfoArray { get; private set; }
	public Dictionary<int, ProjectileInfo> ProjectileinfoDic { get; private set; }
	public void BindProjectileInfoData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(ProjectileinfoArray)).SetValue(this, deserializaedData, null);
		ProjectileinfoDic = ProjectileinfoArray.ToDictionary(i => i.id);
	}
	public ProjectileInfo GetProjectileInfoData(int _id){
		if (ProjectileinfoDic.TryGetValue(_id, out ProjectileInfo value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class StageInfo {
		public int id;
		public string stagename;
		public int needcombatpower;
		public int priorstageid;
	};
	public StageInfo[] StageinfoArray { get; private set; }
	public Dictionary<int, StageInfo> StageinfoDic { get; private set; }
	public void BindStageInfoData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(StageinfoArray)).SetValue(this, deserializaedData, null);
		StageinfoDic = StageinfoArray.ToDictionary(i => i.id);
	}
	public StageInfo GetStageInfoData(int _id){
		if (StageinfoDic.TryGetValue(_id, out StageInfo value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class StageRewardInfo {
		public int id;
		public int order;
		public int rewardtype;
		public int rewardcount;
	};
	public StageRewardInfo[] StagerewardinfoArray { get; private set; }
	public Dictionary<int, StageRewardInfo> StagerewardinfoDic { get; private set; }
	public void BindStageRewardInfoData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(StagerewardinfoArray)).SetValue(this, deserializaedData, null);
		StagerewardinfoDic = StagerewardinfoArray.ToDictionary(i => i.id);
	}
	public StageRewardInfo GetStageRewardInfoData(int _id){
		if (StagerewardinfoDic.TryGetValue(_id, out StageRewardInfo value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
};
