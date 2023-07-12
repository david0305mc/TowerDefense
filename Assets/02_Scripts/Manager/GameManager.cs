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
    public GameObject CharacterItem;

    // object ref
    public GameObject ItemsContainer;

    public Dictionary<int, BaseObj> baseObjDic;

    private void Start()
    {
        baseObjDic = new Dictionary<int, BaseObj>();
        GroundManager.Instance.UpdateAllNodes();
    }

    public Vector3 GetRandomNode()
    { 
        return new Vector3(Random.Range(0, nodeWidth), 0, Random.Range(0, nodeHeight));
    }

    public void SpawnItem()
    {
        BaseObj baseObj = Utill.InstantiateGameObject<BaseObj>(BaseItem, ItemsContainer.transform);
        baseObj.SetPosition(GetRandomNode());
        baseObj.CreateQuad();
    }

    public void SpawnCharacter()
    {
        var baseObj = Utill.InstantiateGameObject<CharacterObj>(CharacterItem, ItemsContainer.transform);
        baseObj.SetPosition(GetRandomNode());
        baseObj.CreateSwordMan();

        baseObj.WalkToPosition(new Vector3(0, 0, 0));
    }

}
