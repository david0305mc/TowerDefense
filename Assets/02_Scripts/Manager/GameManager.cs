using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
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
        BaseObj baseObj = Utill.InstantiateGameObject<BaseObj>(BaseItem, ItemsContainer.transform);
        baseObj.SetPosition(GroundManager.Instance.GetRandomFreePosition());
        baseObj.CreateQuad();
    }

    public void SpawnCharacter()
    {
        int tid = 3;
        var randomePosition = GroundManager.Instance.GetRandomFreePosition();
        var objData = UserData.Instance.CreateBaseObj(tid, (int)randomePosition.x, (int)randomePosition.z);
        var baseObj = CharacterObj.Create(tid, CharacterPrefab, ItemsContainer.transform);
        baseObj.WalkToPosition(new Vector3(0, 0, 0));
        baseObjDic.Add(objData.UID, baseObj);
    }

}
