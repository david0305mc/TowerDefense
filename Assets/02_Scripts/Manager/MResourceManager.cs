using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MResourceManager : SingletonMono<MResourceManager>
{
    private Dictionary<string, ProjectileStraight> projectileDic;
    public Dictionary<string, ProjectileStraight> ProjectileDic => projectileDic;
    public void LoadResources()
    {
        ProjectileStraight[] arrayData =  Resources.LoadAll<ProjectileStraight>("Prefabs/Projectile");
        projectileDic = arrayData.ToDictionary(item => item.name, item => item);
    }

    public ProjectileStraight GetProjectile(string _name)
    {
        if (ProjectileDic.TryGetValue(_name, out ProjectileStraight obj))
        {
            return obj;
        }
        return null;
    }

}
