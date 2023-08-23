#pragma warning disable 114
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public partial class DataManager {
	public partial class ObjTable {
		public int id;
		public OBJ_TYPE object_type;
		public int idle_collectionid;
		public int attack_collectionid;
		public int walk_collectionid;
		public int destroyed_collectionid;
		public string name;
		public string thumbnailpath;
	};
	public ObjTable[] ObjtableArray { get; private set; }
	public Dictionary<int, ObjTable> ObjtableDic { get; private set; }
	public void BindObjTableData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(ObjtableArray)).SetValue(this, deserializaedData, null);
		ObjtableDic = ObjtableArray.ToDictionary(i => i.id);
	}
	public ObjTable GetObjTableData(int _id){
		if (ObjtableDic.TryGetValue(_id, out ObjTable value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class SpriteSheet {
		public int id;
		public int culumns;
		public int rows;
		public int scale;
		public int frame;
		public string respath;
		public string name;
	};
	public SpriteSheet[] SpritesheetArray { get; private set; }
	public Dictionary<int, SpriteSheet> SpritesheetDic { get; private set; }
	public void BindSpriteSheetData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(SpritesheetArray)).SetValue(this, deserializaedData, null);
		SpritesheetDic = SpritesheetArray.ToDictionary(i => i.id);
	}
	public SpriteSheet GetSpriteSheetData(int _id){
		if (SpritesheetDic.TryGetValue(_id, out SpriteSheet value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class SpriteCollection {
		public int id;
		public int b_resid;
		public int br_resid;
		public int r_resid;
		public int tr_resid;
		public int t_resid;
		public string name;
	};
	public SpriteCollection[] SpritecollectionArray { get; private set; }
	public Dictionary<int, SpriteCollection> SpritecollectionDic { get; private set; }
	public void BindSpriteCollectionData(Type type, string text){
		var deserializaedData = CSVDeserialize(text, type);
		GetType().GetProperty(nameof(SpritecollectionArray)).SetValue(this, deserializaedData, null);
		SpritecollectionDic = SpritecollectionArray.ToDictionary(i => i.id);
	}
	public SpriteCollection GetSpriteCollectionData(int _id){
		if (SpritecollectionDic.TryGetValue(_id, out SpriteCollection value)){
			return value;
		}
		UnityEngine.Debug.LogError($"table doesnt contain id {_id}");
		return null;
	}
	public partial class Unitinfo {
		public int id;
		public UNIT_TYPE unit_type;
		public int checkrange;
		public int aggroorder;
		public int defaultgrade;
		public int maxgrade;
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
		public int attackshortdelay;
		public int attacklongdelay;
		public int projectileid;
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
};
