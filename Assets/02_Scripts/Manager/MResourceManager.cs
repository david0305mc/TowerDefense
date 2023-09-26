using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
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

    public Material FlashMaterial;
    public Color FlashColor;
    public AnimationCurve KnockBackCurve;

    private Dictionary<string, ProjectileBase> projectileDic;
    public Dictionary<string, ProjectileBase> ProjectileDic => projectileDic;

    private Dictionary<string, GameObject> prefabDic = new Dictionary<string, GameObject>();
    private Dictionary<string, AsyncOperationHandle<SpriteAtlas>> opAtlasHandleDic = new Dictionary<string, AsyncOperationHandle<SpriteAtlas>>();

    public void LoadResources()
    {
        //ProjectileBase[] arrayData = Resources.LoadAll<ProjectileBase>("Prefabs/Projectile");
        //projectileDic = arrayData.ToDictionary(item => item.name, item => item);
        LoadProjectile().Forget();
        LoadUnits().Forget();
        LoadBoomEffect().Forget();
        LoadAtlas().Forget();
    }

    private async UniTaskVoid LoadAtlas()
    {
        string key = "SpriteAtlas/Atlas_Icon.spriteatlas";
        opAtlasHandleDic[key] = Addressables.LoadAssetAsync<SpriteAtlas>(key);
        await opAtlasHandleDic[key];
    }

    private async UniTaskVoid LoadPopups()
    {
    }

    private async UniTaskVoid LoadProjectile()
    {
        foreach (var item in DataManager.Instance.ProjectileinfoDic)
        {
            prefabDic[item.Value.prefabname] = await Addressables.LoadAssetAsync<GameObject>(item.Value.prefabname);
        }
    }
    private async UniTaskVoid LoadUnits()
    {
        foreach (var item in DataManager.Instance.UnitinfoDic)
        {
            prefabDic[item.Value.prefabname] = await Addressables.LoadAssetAsync<GameObject>(item.Value.prefabname);
        }
    }
    private async UniTaskVoid LoadBoomEffect()
    {
        foreach (var item in DataManager.Instance.UnitgradeinfoArray)
        {
            if (!prefabDic.ContainsKey(item.boomeffectprefab))
            {
                prefabDic[item.boomeffectprefab] = await Addressables.LoadAssetAsync<GameObject>(item.boomeffectprefab);
            }
        }
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

    //public ProjectileBase GetProjectile(string _name)
    //{
    //    if (ProjectileDic.TryGetValue(_name, out ProjectileBase obj))
    //    {
    //        return obj;
    //    }
    //    return null;
    //}

}
