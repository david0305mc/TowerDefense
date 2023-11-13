using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MGameManager : SingletonMono<MGameManager>
{

    public bool PlayNextTutorial(System.Action _endAction = null)
    {
        var tutoInfo = DataManager.Instance.GetTutorialInfoData(UserData.Instance.LocalData.CurrTutorialID);
        if (tutoInfo == null)
            return false;

        switch (tutoInfo.tutotype)
        {
            case TUTO_TYPE.CUTSCENE:
                {
                    var popup = PopupManager.Instance.Show<CutScenePopup>();
                    popup.SetData(int.Parse(tutoInfo.value1), tutoInfo.delay, () =>
                    {
                        OnTutorialEnd(tutoInfo.id, () => {
                            popup.Hide();
                        });
                    });
                }
                break;
            case TUTO_TYPE.DIALOUGE:
                {
                    var popup = PopupManager.Instance.Show<DialoguePopup>();
                    popup.SetData(tutoInfo.id, () =>
                    {
                        OnTutorialEnd(tutoInfo.id, () => {
                            popup.Hide();
                        });
                    });
                }
                break;
            case TUTO_TYPE.CAMERASTAGEMOVE:
                {
                    TouchBlockManager.Instance.AddLock();
                    SetTutorialCamera(int.Parse(tutoInfo.value1), () => {
                        OnTutorialEnd(tutoInfo.id, null);
                        TouchBlockManager.Instance.RemoveLock();
                    });
                }
                break;
            case TUTO_TYPE.NEEDTOUCH:
                {
                    SetTutorialTouchWait(tutoInfo.id, () => {
                        tutorialTouchObj.SetActive(false);
                        OnTutorialEnd(tutoInfo.id, null);
                        _endAction?.Invoke();
                    });
                }
                break;

            case TUTO_TYPE.ATTENDANCE:
                {
                    var popup = PopupManager.Instance.Show<AttendancePopup>(()=> {
                        OnTutorialEnd(tutoInfo.id, () => {
                            
                        });
                    });
                }
                break;
        }
        return true;
    }

    public void OnTutorialEnd(int _prevTutoID, System.Action _hidePrevTuto)
    {
        var prevTutoInfo = DataManager.Instance.GetTutorialInfoData(_prevTutoID);
        UserData.Instance.LocalData.CurrTutorialID++;
        UserData.Instance.SaveLocalData();

        var nextTutoInfo = DataManager.Instance.GetTutorialInfoData(UserData.Instance.LocalData.CurrTutorialID);
        if (nextTutoInfo != null)
        {
            switch (prevTutoInfo.tutotype)
            {
                case TUTO_TYPE.CUTSCENE:
                    if (nextTutoInfo.tutotype != TUTO_TYPE.CUTSCENE)
                    {
                        _hidePrevTuto?.Invoke();
                    }
                    break;
                case TUTO_TYPE.DIALOUGE:
                    if (nextTutoInfo.tutotype != TUTO_TYPE.DIALOUGE)
                    {
                        _hidePrevTuto?.Invoke();
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            _hidePrevTuto?.Invoke();
        }

        switch (prevTutoInfo.tutotype)
        {
            case TUTO_TYPE.CUTSCENE:
                {
                    PlayNextTutorial();
                }
                break;
            case TUTO_TYPE.DIALOUGE:
                {
                    PlayNextTutorial();
                }
                break;
            case TUTO_TYPE.CAMERASTAGEMOVE:
                {
                    PlayNextTutorial();
                }
                break;
            case TUTO_TYPE.NEEDTOUCH:
                {
                    HideTutorialTouchWait();
                    switch (prevTutoInfo.id)
                    {
                        case 7:
                            {
                                mainUI.ShowStageInfo(1, null);
                                worldMap.SelectStage(1);
                                PlayNextTutorial();
                            }
                            break;
                        case 8:
                            {
                                StartStage(1);
                            }
                            break;
                        case 9:
                            {
                            }
                            break;
                        case 11:
                            {
                                mainUI.SelectTab(MainUI.BottomTab.Shop);
                                PlayNextTutorial();
                            }
                            break;
                        case 12:
                            {
                                SummonHero(1, 100, ()=> {
                                    PlayNextTutorial();
                                });
                            }
                            break;
                        case 13:
                            {
                                mainUI.SelectTab(MainUI.BottomTab.Arrangement);
                            }
                            break;
                        case 14:
                            {
                                mainUI.ShowUnitInfo(UserData.Instance.LocalData.TutorialSpawnedUnitUID);
                            }
                            break;
                        case 15:
                            {
                                PlayNextTutorial();
                            }
                            break;
                        case 16:
                            {
                                mainUI.SelectTab(MainUI.BottomTab.Worldmap);
                                PlayNextTutorial();
                            }
                            break;
                        case 18:
                            {
                                mainUI.ShowStageInfo(1, null);
                                worldMap.SelectStage(1);
                                PlayNextTutorial();
                            }
                            break;
                        case 19:
                            {
                                StartStage(1);
                            }
                            break;
                        default:
                            break;
                    }
                }
                break;
        }
    }


    public void SetTutorialCamera(int _stageID, System.Action _endAction)
    {
        WorldMapStageSlot stageSlot = worldMap.GetStageSlotObj(_stageID);
        MCameraManager.Instance.SetFollowObject(stageSlot.CameraPivot, Game.GameConfig.normalTargetDragSpeed, false, Vector2.zero, () =>
        {
            _endAction?.Invoke();
        });
    }

    public void SetTutorialTouchWait(int _tutoID, UnityEngine.Events.UnityAction _endAction)
    {
        tutorialTouchObj.SetActive(true);
        tutorialTouchObj.SetData(_tutoID, _endAction);
    }

    public void HideTutorialTouchWait()
    {
        tutorialTouchObj.SetActive(false);
    }

}
