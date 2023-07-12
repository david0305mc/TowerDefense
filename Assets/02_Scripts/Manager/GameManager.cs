using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
{
    public Material RenderQuadMaterial = default;
    public Texture GridTexture = default;

    public const int nodeWidth = 44;
    public const int nodeHeight = 44;

    // prefab
    public GameObject BaseItem;

    // object ref
    public GameObject ItemsContainer;

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

}
