#pragma warning disable 114
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public partial class DataManager {
	public partial class Localization {
		public string id;
		public string ko;
		public string en;
		public string jp;
	};
	public Localization[] LocalizationArray { get; private set; }
	public Dictionary<string, Localization> LocalizationDic { get; private set; }
	public void BindLocalizationData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(LocalizationArray)).SetValue(this, deserializaedData, null);
		LocalizationDic = LocalizationArray.ToDictionary(i => i.id);
	}
	public Localization GetLocalizationData(string _id){
		if (LocalizationDic.TryGetValue(_id, out Localization value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class Unitinfo {
		public int id;
		public string memo;
		public string memo2;
		public UNIT_TYPE unit_type;
		public int checkrange;
		public int aggroorder;
		public int defaultgrade;
		public int maxgrade;
		public RARITY_TYPE unitrarity;
		public string unitname;
		public string thumbnailpath;
		public string prefabname;
		public string deatheffect;
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
		public string memo;
		public int grade;
		public int combatpower;
		public int hp;
		public int attackdmg;
		public int attackrange;
		public int multiattackcount;
		public int attacklongdelay;
		public int knockback;
		public int summoncnt;
		public int upgradepiececnt;
		public int upgradecostcnt;
		public int dropsoulcnt;
		public int splashdmg;
		public int splashrange;
		public string splasheffectprefab;
		public string boomeffectprefab;
		public int projectileid;
		public int attackcount;
		public int attackshortdelay;
		public int walkspeed;
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
		public int nontarget;
		public int lifetime;
		public int afterhitlifetime;
		public int speed;
		public string prefabname;
		public string memo;
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
		public string prefabname;
		public int stagecleartime;
		public int goldproductamount;
		public int goldproductterm;
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
		public int stageid;
		public int order;
		public ITEM_TYPE rewardtype;
		public int rewardid;
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
	public partial class GachaList {
		public int id;
		public int unitid;
		public int rarity;
		public int count;
		public int weight;
	};
	public GachaList[] GachalistArray { get; private set; }
	public Dictionary<int, GachaList> GachalistDic { get; private set; }
	public void BindGachaListData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(GachalistArray)).SetValue(this, deserializaedData, null);
		GachalistDic = GachalistArray.ToDictionary(i => i.id);
	}
	public GachaList GetGachaListData(int _id){
		if (GachalistDic.TryGetValue(_id, out GachaList value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class WaveStage {
		public int id;
		public int stageid;
		public int time;
		public int unitid;
		public int unitcnt;
		public int unitpowerrate;
	};
	public WaveStage[] WavestageArray { get; private set; }
	public Dictionary<int, WaveStage> WavestageDic { get; private set; }
	public void BindWaveStageData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(WavestageArray)).SetValue(this, deserializaedData, null);
		WavestageDic = WavestageArray.ToDictionary(i => i.id);
	}
	public WaveStage GetWaveStageData(int _id){
		if (WavestageDic.TryGetValue(_id, out WaveStage value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class TutorialInfo {
		public int id;
		public TUTO_TYPE tutotype;
		public string value1;
		public int delay;
		public string memo;
	};
	public TutorialInfo[] TutorialinfoArray { get; private set; }
	public Dictionary<int, TutorialInfo> TutorialinfoDic { get; private set; }
	public void BindTutorialInfoData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(TutorialinfoArray)).SetValue(this, deserializaedData, null);
		TutorialinfoDic = TutorialinfoArray.ToDictionary(i => i.id);
	}
	public TutorialInfo GetTutorialInfoData(int _id){
		if (TutorialinfoDic.TryGetValue(_id, out TutorialInfo value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class Dialogue {
		public int id;
		public string localizekey;
		public int anim;
	};
	public Dialogue[] DialogueArray { get; private set; }
	public Dictionary<int, Dialogue> DialogueDic { get; private set; }
	public void BindDialogueData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(DialogueArray)).SetValue(this, deserializaedData, null);
		DialogueDic = DialogueArray.ToDictionary(i => i.id);
	}
	public Dialogue GetDialogueData(int _id){
		if (DialogueDic.TryGetValue(_id, out Dialogue value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class Attendance {
		public int id;
		public int day;
		public ITEM_TYPE rewardtype;
		public int rewardid;
		public int rewardcount;
	};
	public Attendance[] AttendanceArray { get; private set; }
	public Dictionary<int, Attendance> AttendanceDic { get; private set; }
	public void BindAttendanceData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(AttendanceArray)).SetValue(this, deserializaedData, null);
		AttendanceDic = AttendanceArray.ToDictionary(i => i.id);
	}
	public Attendance GetAttendanceData(int _id){
		if (AttendanceDic.TryGetValue(_id, out Attendance value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class Level {
		public int id;
		public int level;
		public int exp;
		public int unlockslot;
		public int goldreward;
	};
	public Level[] LevelArray { get; private set; }
	public Dictionary<int, Level> LevelDic { get; private set; }
	public void BindLevelData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(LevelArray)).SetValue(this, deserializaedData, null);
		LevelDic = LevelArray.ToDictionary(i => i.id);
	}
	public Level GetLevelData(int _id){
		if (LevelDic.TryGetValue(_id, out Level value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class DevilSay {
		public int id;
		public string saytext;
		public string memo;
		public int showtime;
		public int anim;
	};
	public DevilSay[] DevilsayArray { get; private set; }
	public Dictionary<int, DevilSay> DevilsayDic { get; private set; }
	public void BindDevilSayData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(DevilsayArray)).SetValue(this, deserializaedData, null);
		DevilsayDic = DevilsayArray.ToDictionary(i => i.id);
	}
	public DevilSay GetDevilSayData(int _id){
		if (DevilsayDic.TryGetValue(_id, out DevilSay value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class WorldShipReward {
		public int id;
		public int trycount;
		public ITEM_TYPE rewardtype;
		public int rewardid;
		public int rewardcount;
	};
	public WorldShipReward[] WorldshiprewardArray { get; private set; }
	public Dictionary<int, WorldShipReward> WorldshiprewardDic { get; private set; }
	public void BindWorldShipRewardData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(WorldshiprewardArray)).SetValue(this, deserializaedData, null);
		WorldshiprewardDic = WorldshiprewardArray.ToDictionary(i => i.id);
	}
	public WorldShipReward GetWorldShipRewardData(int _id){
		if (WorldshiprewardDic.TryGetValue(_id, out WorldShipReward value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class PushReward {
		public int id;
		public string title;
		public string message;
		public string time;
		public ITEM_TYPE rewardtype;
		public int rewardid;
		public int rewardcount;
		public string memo1;
		public string memo2;
		public string memo3;
		public string memo4;
		public string memo5;
		public string memo6;
		public string memo7;
	};
	public PushReward[] PushrewardArray { get; private set; }
	public Dictionary<int, PushReward> PushrewardDic { get; private set; }
	public void BindPushRewardData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(PushrewardArray)).SetValue(this, deserializaedData, null);
		PushrewardDic = PushrewardArray.ToDictionary(i => i.id);
	}
	public PushReward GetPushRewardData(int _id){
		if (PushrewardDic.TryGetValue(_id, out PushReward value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
};
