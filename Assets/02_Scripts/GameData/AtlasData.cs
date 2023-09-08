using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "AtlasData", menuName = "GameData/AtlasData")]
public class AtlasData : ScriptableObject
{
    public AssetReferenceGameObject[] atlas;
}
