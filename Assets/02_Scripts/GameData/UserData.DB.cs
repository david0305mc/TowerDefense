
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class UserData : Singleton<UserData>
{

    public UnitData AddHeroData(int _tid, int _count)
    {
        var heroData = LocalData.HeroDataDic.FirstOrDefault(item => item.Value.tid == _tid);
        if (heroData.Equals(default(KeyValuePair<int, UnitData>)))
        {
            var data = UnitData.Create(MGameManager.GenerateUID(), _tid, 1, _count, false);
            LocalData.HeroDataDic.Add(data.uid, data);
            return data;
        }
        else
        {
            heroData.Value.count += _count;
            return heroData.Value;
        }
    }

    public void RemoveHero(int _heroUID)
    {
       LocalData.HeroDataDic.Remove(_heroUID);
    }

}
