using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
public class GameManager : SingletonMono<GameManager>
{
    public Material RenderQuadMaterial = default;
    public Texture GridTexture = default;
    public Texture CharacterTexture = default;
    public Vector3 TestTarget = default;

    public const int nodeWidth = 44;
    public const int nodeHeight = 44;

    // prefab
    public GameObject BaseItem;
    public GameObject CharacterPrefab;

    // object ref
    public GameObject ItemsContainer;

    public Dictionary<int, BaseObj> baseObjDic;
    private static int uidGenerator = 1000;

    private void Start()
    {
        InitGame();
    }
    public static int GenerateUID()
    {
        return uidGenerator++;
    }

    public void InitGame()
    {
        baseObjDic = new Dictionary<int, BaseObj>();
        GroundManager.Instance.UpdateAllNodes();
    }

    public void SpawnItem()
    {
        int tid = 1;
        var randomePosition = GroundManager.Instance.GetRandomFreePosition();
        var objData = UserData.Instance.CreateBaseObj(tid, (int)randomePosition.x, (int)randomePosition.z);
        var baseObj = BaseObj.Create(objData, BaseItem, ItemsContainer.transform);
        baseObjDic.Add(objData.UID, baseObj);
    }

    public void SpawnCharacter()
    {
        int tid = 3;
        var randomePosition = GroundManager.Instance.GetRandomFreePosition();
        var objData = UserData.Instance.CreateBaseObj(tid, (int)randomePosition.x, (int)randomePosition.z);
        objData.ObjStatus = ObjStatus.Walk;
        CharacterObj baseObj = (CharacterObj)BaseObj.Create(objData, CharacterPrefab, ItemsContainer.transform);
        baseObj.WalkToPosition(TestTarget);
        baseObjDic.Add(objData.UID, baseObj);
    }

}
