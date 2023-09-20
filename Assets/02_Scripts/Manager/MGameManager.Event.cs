using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        return slotIndex;
    }

    public void RemoveBattleParty(int _slotIndex)
    {
        UserData.Instance.RemoveBattleParty(_slotIndex);
        UserData.Instance.SaveLocalData();
    }

    public void WinStage()
    {
        RemoveAllBattleHero();
        RemoveStage();
        RemoveAllProjectile();

        cameraManager.SetZoomAndSize(2, 7, -2, 2, -2, 2);
        UserData.Instance.ClearStage(UserData.Instance.CurrStage);
        worldMap.gameObject.SetActive(true);
        worldMap.UpdateWorld();
        AddSoul(10);
    }
}
