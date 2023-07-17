using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
public class BaseObjData
{
    public int UID { get; set; }
    public int TID { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public ObjStatus ObjStatus { get; set; }
    public GameType.Direction Direction { get; set; }
    public DataManager.ObjTable RefObjData { get; private set; }

    public static BaseObjData Create(int uid, int tid, int x, int y)
    {
        BaseObjData data = new BaseObjData();
        data.RefObjData = DataManager.Instance.GetObjTableData(tid);
        data.UID = uid;
        data.TID = tid;
        data.X = x;
        data.Y = y;
        data.ObjStatus = ObjStatus.Idle;
        data.Direction = GameType.Direction.BOTTOM_RIGHT;
        return data;
    }
}