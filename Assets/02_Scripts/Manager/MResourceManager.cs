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

    }

    private async UniTaskVoid LoadProjectile()
    {
        foreach (var item in DataManager.Instance.ProjectileinfoDic)
        {
            prefabDic[item.Value.prefabname] = await Addressables.LoadAssetAsync<GameObject>(item.Value.prefabname);
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

    //public ProjectileBase GetProjectile(string _name)
    //{
    //    if (ProjectileDic.TryGetValue(_name, out ProjectileBase obj))
    //    {
    //        return obj;
    //    }
    //    return null;
    //}

}
