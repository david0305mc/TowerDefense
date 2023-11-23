using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
using Cysharp.Threading.Tasks;

public partial class MGameManager : SingletonMono<MGameManager>
{
    //유저 데이터가 바뀌는 액션

    public void AddSoul(int _add, bool forceSave = true)
    {
        UserData.Instance.LocalData.Soul.Value += _add;
        if (forceSave)
        {
            UserData.Instance.SaveLocalData();
        }
    }

    public void AddGold(int _add, bool _forceSave = true)
    {
        UserData.Instance.LocalData.Gold.Value += _add;
        if (_forceSave)
        {
            UserData.Instance.SaveLocalData();
        }
    }
    public void AddStageRewards(int _soul, List<DataManager.StageRewardInfo> rewards)
    {
        UserData.Instance.LocalData.Soul.Value += _soul;

        foreach (var item in rewards)
        {
            switch ((ITEM_TYPE)item.rewardtype)
            {
                case ITEM_TYPE.EXP:
                    AddExp(item.rewardcount);
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

    public void ReceiveReward(List<RewardData> _rewardList)
    {
        foreach (var item in _rewardList)
        {
            switch (item.rewardtype)
            {
                case ITEM_TYPE.EXP:
                    AddExp(item.rewardcount);
                    break;
                case ITEM_TYPE.SOUL:
                    AddSoul(item.rewardcount, false);
                    break;
                case ITEM_TYPE.UNIT:
                    UserData.Instance.AddHeroData(item.rewardid, item.rewardcount);
                    break;
                case ITEM_TYPE.STAMINA:
                    AddStamina(item.rewardcount, true);
                    break;
                case ITEM_TYPE.GOLD:
                    AddGold(item.rewardcount, false);
                    break;
            }
        }
    }
    public async UniTask ReceiveOfflineReward()
    {
        UniTaskCompletionSource ucs = new UniTaskCompletionSource();
        if (UserData.Instance.OfflineTimeSeconds > Game.GameConfig.OfflineRewardMinSecond)
        {
            var popup = PopupManager.Instance.Show<OfflineRewardPopup>();
            popup.SetData(_gold => {

                List<RewardData> rewardList = new List<RewardData>();
                rewardList.Add(new RewardData()
                {
                    rewardtype = ITEM_TYPE.GOLD,
                    rewardid = 0,
                    rewardcount = _gold,
                });
                ReceiveReward(rewardList);
                UserData.Instance.OfflineTimeSeconds = 0;
                UserData.Instance.SaveLocalData();
                ucs.TrySetResult();
            });
        }
        else
        {
            UserData.Instance.OfflineTimeSeconds = 0;
            UserData.Instance.SaveLocalData();
            ucs.TrySetResult();
        }
        await ucs.Task;
    }
    public async UniTask ReceivePushReward()
    {
        UniTaskCompletionSource ucs = new UniTaskCompletionSource();
        var pushRewardInfo = DataManager.Instance.GetAvailablePushReward();
        if (pushRewardInfo != null)
        {
            List<RewardData> rewardList = new List<RewardData>();
            rewardList.Add(new RewardData()
            {
                rewardtype = pushRewardInfo.rewardtype,
                rewardid = pushRewardInfo.rewardid,
                rewardcount = pushRewardInfo.rewardcount,
            });
            ReceiveReward(rewardList);
            var popup = PopupManager.Instance.Show<PushRewardPopup>(() => {
                ucs.TrySetResult();
            });
            popup.SetData(rewardList);
            System.DateTime rewardTime = System.DateTime.Parse(pushRewardInfo.time);
            var timeStamp = Utill.ConvertToUnitxTimeStamp(rewardTime);
            UserData.Instance.LocalData.LastPushRewardedTime = (long)timeStamp;
            UserData.Instance.SaveLocalData();
        }
        else
        {
            ucs.TrySetResult();
        }
        await ucs.Task;
    }

    public void ReceiveShipReward(int _id)
    {
        var shipRewardInfo = DataManager.Instance.GetWorldShipRewardData(_id);
        
        List<RewardData> rewardList = new List<RewardData>();
        rewardList.Add(new RewardData()
        {
            rewardtype = shipRewardInfo.rewardtype,
            rewardid = shipRewardInfo.rewardid,
            rewardcount = shipRewardInfo.rewardcount,
        });
        ReceiveReward(rewardList);
        var popup = PopupManager.Instance.Show<RewardPopup>();
        popup.SetData(rewardList);
        UserData.Instance.LocalData.ShipRewardID = _id + 1;
        UserData.Instance.SaveLocalData();
    }
    public void ReceiveAttendanceReward(int _day)
    {
        var attendanceInfoLists = DataManager.Instance.GetAttendanceInfosByDay(_day);

        List<RewardData> rewardList = new List<RewardData>();
        foreach (var item in attendanceInfoLists)
        {
            rewardList.Add(new RewardData()
            {
                rewardtype = item.rewardtype,
                rewardid = item.rewardid,
                rewardcount = item.rewardcount,
            });
        }
        ReceiveReward(rewardList);
        var popup = PopupManager.Instance.Show<RewardPopup>();
        popup.SetData(rewardList);

        UserData.Instance.LocalData.AttendanceRewardedDic[_day] = 1;
        UserData.Instance.LocalData.NextAttendanceTime = GameTime.GetLocalMidnight();

        int maxDay = DataManager.Instance.AttendanceDic.Values.Max(item => item.day);
        if (maxDay == UserData.Instance.LocalData.AttendanceDay)
        {
            if (!UserData.Instance.HasAttendacneReward())
            {
                UserData.Instance.LocalData.AttendanceDay = 0;
                UserData.Instance.LocalData.AttendanceRewardedDic.Clear();
            }
        }
        UserData.Instance.SaveLocalData();
        MessageDispather.Publish(EMessage.Update_Attendance);
    }

    public void AddStageGold(int _stageID)
    {
        var stageData = UserData.Instance.GetStageData(_stageID);
        AddGold(stageData.refData.goldproductamount, true);
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
            var unitData = UserData.Instance.AddHeroData(gachaInfo.unitid, gachaInfo.count);

            if (UserData.Instance.IsOnTutorial())
            {
                UserData.Instance.LocalData.TutorialSpawnedUnitUID = unitData.uid;
            }
        }
        var popup = PopupManager.Instance.Show<GachaResultPopup>(_hideAction);
        popup.SetData(gachaList);
        UserData.Instance.LocalData.Gold.Value -= _goldCost;
        UserData.Instance.SaveLocalData();
    }
    public void BuyStamina(int _stamina, int _goldCost)
    {
        UserData.Instance.LocalData.Gold.Value -= _goldCost;
        var receiveLists = new List<RewardData>() { new RewardData() { rewardtype = ITEM_TYPE.STAMINA, rewardcount = _stamina, rewardid = -1 } };
        ReceiveReward(receiveLists);
        var popup = PopupManager.Instance.Show<RewardPopup>();
        popup.SetData(receiveLists);
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
                if (UserData.Instance.LocalData.Stamina.Value < ConfigTable.Instance.StaminaMaxCount)
                {
                    UserData.Instance.LocalData.Stamina.Value = ConfigTable.Instance.StaminaMaxCount;
                }
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

    public void AddExp(int _exp)
    {
        int prevLevel = DataManager.Instance.ConvertExpToLevel(UserData.Instance.LocalData.Exp.Value);
        UserData.Instance.LocalData.Exp.Value += _exp;
        int currLevel = DataManager.Instance.ConvertExpToLevel(UserData.Instance.LocalData.Exp.Value);
        if (prevLevel < currLevel)
        {
            var levelInfo = DataManager.Instance.GetLevelData(currLevel);
            UserData.Instance.LocalData.Level.Value = currLevel;
            UserData.Instance.LocalData.Gold.Value += levelInfo.goldreward;
            UserData.Instance.LocalData.UnitSlotCount.Value = levelInfo.unlockslot;
            AddStamina(ConfigTable.Instance.StaminaMaxCount, false);
        }
    }

    public void WinStage()
    {
        TouchBlockManager.Instance.AddLock();
        gameState = Game.GameConfig.GameState.GameEnd;
        var stageRewards = DataManager.Instance.GetStageRewards(UserData.Instance.PlayingStage);
        int prevLevel = UserData.Instance.LocalData.Level.Value;
        AddStageRewards(UserData.Instance.AcquireSoul.Value, stageRewards);
        int currLevel = UserData.Instance.LocalData.Level.Value;
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

        if (prevLevel < currLevel)
        {
            UniTask.Create(async () =>
            {
                await UniTask.WaitForSeconds(1f);
                var popup = PopupManager.Instance.Show<LevelUpPopup>();
                popup.SetData(currLevel);
                TouchBlockManager.Instance.RemoveLock();
            });
        }
        else
        {
            TouchBlockManager.Instance.RemoveLock();
        }
    }
    public void LoseStage()
    {
        gameState = Game.GameConfig.GameState.GameEnd;
        DisposeCTS();
        SetAllUnitEndState();
        List<DataManager.StageRewardInfo> stageRewards = new List<DataManager.StageRewardInfo>();
        AddStageRewards(UserData.Instance.AcquireSoul.Value, stageRewards);
        var popup = PopupManager.Instance.Show<GameResultPopup>();
        popup.SetData(false, stageRewards, () =>
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
            PopupManager.Instance.ShowSystemOneBtnPopup(LocalizeManager.Instance.GetLocalString("maxGrade"), "OK");
            return;
        }

        if (heroData.refUnitGradeData.upgradepiececnt > heroData.count)
        {
            PopupManager.Instance.ShowSystemOneBtnPopup(LocalizeManager.Instance.GetLocalString("morePiece"), "OK");
            return;
        }

        if (heroData.refUnitGradeData.upgradecostcnt > UserData.Instance.LocalData.Soul.Value)
        {
            PopupManager.Instance.ShowSystemOneBtnPopup(LocalizeManager.Instance.GetLocalString("moreSoul"), "OK");
            return;    
        }

        UserData.Instance.UpgradeHero(_uid);
        UserData.Instance.SaveLocalData();
        MessageDispather.Publish(EMessage.Update_HeroParty);
    }

}
