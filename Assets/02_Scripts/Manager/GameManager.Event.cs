using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager : SingletonMono<GameManager>
{
    public void SpawnBaseObjEvent(int tid, int x, int y, bool isEnemy, Game.ObjStatus status = Game.ObjStatus.Idle)
    {
        var objData = UserData.Instance.CreateBaseObjData(tid, x, y, isEnemy);
        objData.ObjStatus = status;
        var obj = SpawnObject(objData.UID);
        obj.StartStateMachine();
        UserData.Instance.SaveLocalData();
        
    }

    public void MoveBaseObjEvent(int uid, Vector3 pos)
    {
        UserData.Instance.LocalData.BaseObjDic[uid].X = (int)pos.x;
        UserData.Instance.LocalData.BaseObjDic[uid].Y = (int)pos.z;
        UserData.Instance.SaveLocalData();
    }

    public void DetroyEnemy(int uid)
    {
        UserData.Instance.RemoveObj(uid);
        UserData.Instance.SaveLocalData();
        RemoveObject(uid);
    }

}
