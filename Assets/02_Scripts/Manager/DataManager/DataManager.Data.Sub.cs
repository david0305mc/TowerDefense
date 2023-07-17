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

}
