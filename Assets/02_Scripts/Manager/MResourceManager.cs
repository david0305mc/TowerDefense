using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using UnityEngine.ResourceManagement.AsyncOperations;

public class MResourceManager : SingletonMono<MResourceManager>
{

    [System.Serializable]
    public class AssetReferenceAtlas : AssetReferenceT<SpriteAtlas>
    {
        public AssetReferenceAtlas(string guid) : base(guid) { }
    }

    [SerializeField] private SpriteAtlas commonAtlas;
    [SerializeField] private SpriteAtlas iconAtlas;

    public Material FlashMaterial;
    public Color FlashColor;
    public Color RarityColorCommon;
    public Color RarityColorRare;
    public Color RarityColorEpic;
    public Color RarityColorLegendary;
    public List<Image> RarityBG;
    public AnimationCurve KnockBackCurve;
    public Sprite SoulSprite;
    public Sprite ExpSprite;

    private Dictionary<string, ProjectileBase> projectileDic;
    public Dictionary<string, ProjectileBase> ProjectileDic => projectileDic;

    private Dictionary<string, GameObject> prefabDic = new Dictionary<string, GameObject>();
    private Dictionary<string, AsyncOperationHandle<SpriteAtlas>> opAtlasHandleDic = new Dictionary<string, AsyncOperationHandle<SpriteAtlas>>();

    public async UniTask LoadResources()
    {
        List<UniTask> taskLists = new List<UniTask>();
        taskLists.Add(LoadProjectile());
        taskLists.Add(LoadUnits());
        taskLists.Add(LoadBoomEffect());
        taskLists.Add(LoadAtlas());
        await UniTask.WhenAll(taskLists);
    }

    private async UniTask LoadAtlas()
    {
        string key = "SpriteAtlas/Atlas_Icon.spriteatlas";
        opAtlasHandleDic[key] = Addressables.LoadAssetAsync<SpriteAtlas>(key);
        await opAtlasHandleDic[key];
    }

    private async UniTaskVoid LoadPopups()
    {
    }

    private async UniTask LoadProjectile()
    {
        foreach (var item in DataManager.Instance.ProjectileinfoDic)
        {
            prefabDic[item.Value.prefabname] = await Addressables.LoadAssetAsync<GameObject>(item.Value.prefabname);
        }
    }
    private async UniTask LoadUnits()
    {
        foreach (var item in DataManager.Instance.UnitinfoDic)
        {
            prefabDic[item.Value.prefabname] = await Addressables.LoadAssetAsync<GameObject>(item.Value.prefabname);
        }
    }
    private async UniTask LoadBoomEffect()
    {
        foreach (var item in DataManager.Instance.UnitgradeinfoArray)
        {
            if (!prefabDic.ContainsKey(item.boomeffectprefab))
            {
                prefabDic[item.boomeffectprefab] = await Addressables.LoadAssetAsync<GameObject>(item.boomeffectprefab);
            }
        }
    }

    public Sprite GetBuildAtlas(string _name) 
    {
        var sprite = commonAtlas.GetSprite(_name);
        if (sprite != null)
        {
            return sprite;
        }
        sprite = iconAtlas.GetSprite(_name);
        if (sprite != null)
        {
            return sprite;
        }
        return null;
    }
    public Sprite GetSpriteFromAtlas(string _str)
    {
        foreach (var item in opAtlasHandleDic)
        {
            var sprite = item.Value.Result.GetSprite(_str);
            if (sprite != null)
            {
                return sprite;
            }
        }
        return null;
    }
    public ProjectileBase GetProjectile(string _name)
    {
        if (prefabDic.TryGetValue(_name, out GameObject obj))
        {
            return obj.GetComponent<ProjectileBase>();
        }
        return null;
    }
    public GameObject GetPrefab(string _name)
    {
        if (prefabDic.TryGetValue(_name, out GameObject obj))
        {
            return obj;
        }
        Debug.LogError($"Faild To GetPrefab {_name}");
        return null;
    }

    public Color GetRarityColor(RARITY_TYPE _rarityType)
    {
        switch (_rarityType)
        {
            case RARITY_TYPE.COMMON:
                return RarityColorCommon;
            case RARITY_TYPE.RARE:
                return RarityColorRare;
            case RARITY_TYPE.EPIC:
                return RarityColorEpic;
            case RARITY_TYPE.LEGENDARY:
                return RarityColorLegendary;
        }
        return RarityColorCommon;
    }

    //public ProjectileBase GetProjectile(string _name)
    //{
    //    if (ProjectileDic.TryGetValue(_name, out ProjectileBase obj))
    //    {
    //        return obj;
    //    }
    //    return null;
    //}

}
