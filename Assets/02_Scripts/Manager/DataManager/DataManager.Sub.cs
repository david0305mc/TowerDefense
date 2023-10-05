using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class DataManager
{
    public void MakeClientDT()
    {

    }
    public int MaxStage()
    {
        return StageinfoDic.OrderByDescending(item => item.Value.id).First().Key;
    }
    public UnitGradeInfo GetUnitGrade(int _unitID, int _grade)
    {
        var data = UnitgradeinfoDic.FirstOrDefault(item => item.Value.unitid == _unitID && item.Value.grade == _grade);
        if (!data.Equals(default(KeyValuePair<int, UnitGradeInfo>)))
        {
            return data.Value;
        }
        return null;
    }

    public List<int> GenerateGachaResultList(int _count)
    {
        List<int> gachaList = new List<int>();
        for (int i = 0; i < _count; i++)
        {
            gachaList.Add(GenerateGachaResult());
        }
        return gachaList;
    }

    public int GenerateGachaResult()
    {
        int sum = GachalistArray.Sum(item => item.weight);
        int randNum = Random.Range(0, sum);

        for (int i = 0; i < GachalistArray.Length; i++)
        {
            if (randNum <= 0)
            {
                return GachalistArray[i].id;
            }
            randNum -= GachalistArray[i].weight;
        }
        return GachalistArray.Last().id;
    }

    public List<StageRewardInfo> GetStageRewards(int _stageID)
    {
        return StagerewardinfoDic.Values.Where(item => item.stageid == _stageID).ToList();
    }


}
