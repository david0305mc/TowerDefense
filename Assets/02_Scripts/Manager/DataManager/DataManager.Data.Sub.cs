using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Game;
public partial class DataManager
{
	public partial class ObjTable
	{
		public int GetCollectionID(ObjStatus _status)
		{
			switch (_status)
			{
				case ObjStatus.Idle:
					return idle_collectionid;

				case ObjStatus.Walk:
					return walk_collectionid;

				case ObjStatus.Attack:
					return attack_collectionid;
			}
			return idle_collectionid;
		}
	};

	public partial class SpriteCollection
	{
		public int GetSpriteCollection(GameType.Direction _direction)
		{
			switch (_direction)
			{
				case GameType.Direction.BOTTOM:
					return b_resid;
				case GameType.Direction.BOTTOM_LEFT:
				case GameType.Direction.BOTTOM_RIGHT:
					return br_resid;

				case GameType.Direction.LEFT:
				case GameType.Direction.RIGHT:
					return r_resid;

				case GameType.Direction.TOP_LEFT:
				case GameType.Direction.TOP_RIGHT:
					return tr_resid;

				case GameType.Direction.TOP:
					return t_resid;
			}
			return br_resid;
		}
	};

}
