using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public partial class MGameManager : SingletonMono<MGameManager>
{
    //���� �����Ͱ� �ٲ�� �׼�

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
        RemoveAllBattleHero();
        RemoveStage();
        RemoveAllProjectile();

        cameraManager.SetZoomAndSize(2, 7, -2, 9, -2, 6);
        UserData.Instance.ClearStage(UserData.Instance.CurrStage);
        worldMap.gameObject.SetActive(true);
        worldMap.UpdateWorld();
        AddSoul(10);
        spawnHeroCts?.Cancel();
        cts?.Cancel();
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
