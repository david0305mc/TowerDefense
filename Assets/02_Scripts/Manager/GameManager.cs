using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
public partial class GameManager : SingletonMono<GameManager>
{
    public Material RenderQuadMaterial = default;
    public Texture GridTexture = default;
    public Texture CharacterTexture = default;

    public const int nodeWidth = 44;
    public const int nodeHeight = 44;

    // prefab
    public GameObject BaseItem;
    public GameObject CharacterPrefab;

    // object ref
    public GameObject ItemsContainer;

    public Dictionary<int, BaseObj> baseObjDic;

    private void Start()
    {
        InitGame();
    }
    public static int GenerateUID()
    {
        return UserData.Instance.LocalData.uidSeed++;
    }

    public void InitGame()
    {
        baseObjDic = new Dictionary<int, BaseObj>();
        GroundManager.Instance.UpdateAllNodes();

        foreach (var item in UserData.Instance.LocalData.BaseObjDic)
        {
            SpawnObject(item.Key);
        }

        foreach (var item in UserData.Instance.LocalData.BaseObjDic)
        {
            baseObjDic[item.Key].StartStateMachine();
        }
    }

    private BaseObj SpawnObject(int uid)
    {
        var objData = UserData.Instance.LocalData.BaseObjDic[uid];
        CharacterObj baseObj = (CharacterObj)BaseObj.Create(objData, CharacterPrefab, ItemsContainer.transform);
        baseObjDic.Add(objData.UID, baseObj);
        return baseObj;
    }

    private void RemoveObject(int uid)
    {
        Destroy(baseObjDic[uid].gameObject);
        baseObjDic.Remove(uid);
        Debug.Log($"RemoveObject {uid}");
    }

    public BaseObj GetnearestTarget(int uid)
    {
        var orgObj = baseObjDic[uid];
        BaseObj nearestObj = null;
        float shortDist = float.MaxValue;
        foreach (var item in baseObjDic)
        {
            float dist = Vector3.Distance(orgObj.transform.position, item.Value.transform.position);
            if (uid == item.Key || !item.Value.BaseObjData.IsEnemy)
                continue;

            if (dist < shortDist)
            {
                shortDist = dist;
                nearestObj = item.Value;
            }
        }

        return nearestObj;
    }
}
