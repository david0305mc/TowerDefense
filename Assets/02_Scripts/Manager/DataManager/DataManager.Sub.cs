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

    public List<WaveStage> GetWaveInfoList(int _stage)
    {
        return WavestageArray.Where(item => item.stageid == _stage).OrderBy(i => i.id).ToList();
    }

    public List<Attendance> GetAttendanceInfosByDay(int _day)
    {
        return AttendanceDic.Values.Where(item => item.day == _day).ToList();
    }

    public int ConvertExpToLevel(long _exp)
    {
        foreach (var item in LevelArray)
        {
            if (_exp < item.exp)
                return item.level;
        }
        return LevelArray.Max(item => item.level);
    }

    public int GetUnlockLevelBySlotIndex(int _slotIndex)
    {
        var levelDataList = LevelDic.Values.Where(item => item.unlockslot == _slotIndex).ToList();

        if (levelDataList.Count > 0)
        {
            return levelDataList.OrderBy(item => item.id).First().level;
        }
        return -1;
    }
}
