using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public partial class MGameManager : SingletonMono<MGameManager>
{
    //유저 데이터가 바뀌는 액션

    public void AddSoul(int _add)
    {
        UserData.Instance.LocalData.Soul.Value += _add;
        UserData.Instance.SaveLocalData();
    }

    public void AddHero(int _tid, int _count)
    {
        UserData.Instance.AddHeroData(_tid, _count);
        UserData.Instance.SaveLocalData();
    }

    public void RemoveHero(int _heroUID)
    {
        UserData.Instance.RemoveHero(_heroUID);
        UserData.Instance.SaveLocalData();
    }
    public int AddBattleParty(int _heroUID)
    {
        int slotIndex = UserData.Instance.AddBattleParty(_heroUID);
        UserData.Instance.SaveLocalData();
        MessageDispather.Publish(EMessage.Update_UserData);
        return slotIndex;
    }

    public void RemoveBattleParty(int _slotIndex)
    {
        UserData.Instance.RemoveBattleParty(_slotIndex);
        MessageDispather.Publish(EMessage.Update_UserData);
        UserData.Instance.SaveLocalData();
    }

    public void WinStage()
    {
        UserData.Instance.ClearStage(UserData.Instance.CurrStage);
        AddSoul(10);
    }

    public void UpgradeUnit(int _uid)
    {
        var heroData = UserData.Instance.GetHeroData(_uid);
        if (heroData.IsMaxGrade)
        {
            PopupManager.Instance.ShowSystemOneBtnPopup("Max Grade", "OK");
            return;
        }

        if (heroData.refUnitGradeData.upgradepiececnt > UserData.Instance.LocalData.Soul.Value)
        {
            PopupManager.Instance.ShowSystemOneBtnPopup("Soul is not enough", "OK");
            return;    
        }

        UserData.Instance.UpgradeHero(_uid);
        UserData.Instance.SaveLocalData();
        MessageDispather.Publish(EMessage.Update_UserData);
    }

}
