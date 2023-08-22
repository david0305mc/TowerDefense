using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public partial class DataManager 
{
    public void MakeClientDT()
    {

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

}
