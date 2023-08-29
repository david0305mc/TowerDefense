using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MResourceManager : SingletonMono<MResourceManager>
{
    public Material FlashMaterial;
    public Color FlashColor;

    private Dictionary<string, ProjectileBase> projectileDic;
    public Dictionary<string, ProjectileBase> ProjectileDic => projectileDic;

    public void LoadResources()
    {
        ProjectileBase[] arrayData =  Resources.LoadAll<ProjectileBase>("Prefabs/Projectile");
        projectileDic = arrayData.ToDictionary(item => item.name, item => item);
    }

    public ProjectileBase GetProjectile(string _name)
    {
        if (ProjectileDic.TryGetValue(_name, out ProjectileBase obj))
        {
            return obj;
        }
        return null;
    }

}
