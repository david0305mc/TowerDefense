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

    public void AddStageRewards(int _soul, List<DataManager.StageRewardInfo> rewards)
    {
        UserData.Instance.LocalData.Soul.Value += _soul;

        foreach (var item in rewards)
        {
            switch ((ITEM_TYPE)item.rewardtype)
            {
                case ITEM_TYPE.EXP:
                    UserData.Instance.LocalData.Exp.Value += item.rewardcount;
                    break;
                case ITEM_TYPE.SOUL:
                    UserData.Instance.LocalData.Soul.Value += item.rewardcount;
                    break;
                case ITEM_TYPE.UNIT:
                    UserData.Instance.AddHeroData(item.rewardid, item.rewardcount);
                    break;
                default:
                    break;
            }
        }
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
        gameState = Game.GameConfig.GameState.GameEnd;
        cameraManager.EnableCameraControl = true;
        var stageInfo = DataManager.Instance.GetStageInfoData(UserData.Instance.PlayingStage);
        var stageRewards = DataManager.Instance.GetStageRewards(UserData.Instance.PlayingStage);

        AddStageRewards(UserData.Instance.AcquireGold.Value, stageRewards);
        UserData.Instance.ClearStage(UserData.Instance.PlayingStage);

        var popup = PopupManager.Instance.Show<GameResultPopup>();
        popup.SetData(true, stageRewards, () =>
        {
            RemoveStage();
            BackToWorld();
        }, () =>
        {
            RemoveStage();
            RetryStage();
        }, () =>
        {
            RemoveStage();
            NextStage();
        });
    }
    public void LoseStage()
    {
        gameState = Game.GameConfig.GameState.GameEnd;
        DisposeCTS();
        SetAllUnitEndState();
        var popup = PopupManager.Instance.Show<GameResultPopup>();
        popup.SetData(false, null, () =>
        {
            RemoveStage();
            BackToWorld();
        }, () =>
        {
            RemoveStage();
            RetryStage();
        }, () =>
        {
            RemoveStage();
            NextStage();
        });
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
