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

    public void CheckStageGold(int _stageID, Vector3 _targetPos)
    {
        var stageData = UserData.Instance.GetStageData(_stageID);
        if (UserData.Instance.GetStageStatus(_stageID) == Game.StageStatus.Occupation)
        {
            if (stageData.goldharvestTime.Value <= GameTime.Get())
            {
                stageData.GenerateharvestTime();
                var soulObj = Lean.Pool.LeanPool.Spawn(goldRewardPrefab, _targetPos, Quaternion.identity, objRoot);
                soulObj.Shoot(mainUI.GoldTarget, () => {
                    AddStageGold(_stageID);
                });
            }
        }
    }
    public void AddStageGold(int _stageID)
    {
        var stageData = UserData.Instance.GetStageData(_stageID);

        UserData.Instance.LocalData.Gold.Value += stageData.refData.goldproductamount;
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
        MessageDispather.Publish(EMessage.Update_HeroParty);
        return slotIndex;
    }

    public void RemoveBattleParty(int _slotIndex)
    {
        UserData.Instance.RemoveBattleParty(_slotIndex);
        MessageDispather.Publish(EMessage.Update_HeroParty);
        UserData.Instance.SaveLocalData();
    }

    public void SummonHero(int _count, int _goldCost, System.Action _hideAction = null)
    {
        var gachaList = DataManager.Instance.GenerateGachaResultList(_count);
        foreach (var item in gachaList)
        {
            var gachaInfo = DataManager.Instance.GetGachaListData(item);
            UserData.Instance.AddHeroData(gachaInfo.unitid, _count);
        }
        var popup = PopupManager.Instance.Show<GachaResultPopup>(_hideAction);
        popup.SetData(gachaList);
        UserData.Instance.LocalData.Gold.Value -= _goldCost;
        UserData.Instance.SaveLocalData();
    }
    public void BuyStamina(int _stamina, int _goldCost)
    {
        UserData.Instance.LocalData.Gold.Value -= _goldCost;
        AddStamina(_stamina, true);
        UserData.Instance.SaveLocalData();
    }

    private void AddStamina(int _count, bool _force)
    {
        if (UserData.Instance.LocalData.Stamina.Value + _count >= ConfigTable.Instance.StaminaMaxCount)
        {
            if (_force)
            {
                UserData.Instance.LocalData.Stamina.Value += _count;
            }
            else
            {
                UserData.Instance.LocalData.Stamina.Value = ConfigTable.Instance.StaminaMaxCount;
            }
            
            UserData.Instance.LocalData.StaminaLastSpawnTime = GameTime.Get();
        }
        else
        {
            UserData.Instance.LocalData.Stamina.Value += _count;
            UserData.Instance.LocalData.StaminaLastSpawnTime += (_count * ConfigTable.Instance.StaminaChargeTime);
        }
    }

    public void ConsumeStamina(int _stamina)
    {
        if (UserData.Instance.LocalData.Stamina.Value >= ConfigTable.Instance.StaminaMaxCount)
        {
            UserData.Instance.LocalData.StaminaLastSpawnTime = GameTime.Get();
        }
        UserData.Instance.LocalData.Stamina.Value -= _stamina;
        UserData.Instance.SaveLocalData();
    }
    public void WinStage()
    {
        gameState = Game.GameConfig.GameState.GameEnd;
        cameraManager.EnableCameraControl = true;
        var stageInfo = DataManager.Instance.GetStageInfoData(UserData.Instance.PlayingStage);
        var stageRewards = DataManager.Instance.GetStageRewards(UserData.Instance.PlayingStage);

        AddStageRewards(UserData.Instance.AcquireSoul.Value, stageRewards);
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
        MessageDispather.Publish(EMessage.Update_HeroParty);
    }

}
