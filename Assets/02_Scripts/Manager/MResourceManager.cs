using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class MResourceManager : SingletonMono<MResourceManager>
{
    public Material FlashMaterial;
    public Color FlashColor;

    private Dictionary<string, ProjectileBase> projectileDic;
    public Dictionary<string, ProjectileBase> ProjectileDic => projectileDic;

    private Dictionary<string, GameObject> prefabDic = new Dictionary<string, GameObject>();
    public void LoadResources()
    {
        //ProjectileBase[] arrayData = Resources.LoadAll<ProjectileBase>("Prefabs/Projectile");
        //projectileDic = arrayData.ToDictionary(item => item.name, item => item);
        LoadProjectile().Forget();
        LoadUnits().Forget();
        LoadBoomEffect().Forget();
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
