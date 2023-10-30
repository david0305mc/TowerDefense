using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    protected override void init()
    {
        base.init();
    }

    public void Play()
    {
        var tutoInfo = DataManager.Instance.GetTutorialInfoData(UserData.Instance.LocalData.CurrTutorialID);
        if (tutoInfo == null)
            return;

        switch (tutoInfo.tutotype)
        {
            case TUTO_TYPE.CUTSCENE:
                {
                    var popup = PopupManager.Instance.Show<CutScenePopup>();
                    popup.SetData(int.Parse(tutoInfo.value1), () =>
                    {
                        PlayEnd(tutoInfo.id, ()=> {
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
                        PlayEnd(tutoInfo.id, ()=> {
                            popup.Hide();
                        });
                    });
                }
                break;
            case TUTO_TYPE.CAMERASTAGEMOVE:
                {
                    MGameManager.Instance.SetTutorialCamera(int.Parse(tutoInfo.value1), () => {
                        PlayEnd(tutoInfo.id, null);
                    });
                }
                break;
            case TUTO_TYPE.NEEDTOUCH:
                {
                    MGameManager.Instance.SetTutorialTouchWait(tutoInfo.id, ()=> {
                        PlayEnd(tutoInfo.id, null);
                    });
                }
                break;
        }
    }

    public void PlayEnd(int _prevTutoID, System.Action _hidePrevTuto)
    {
        var prevTutoInfo = DataManager.Instance.GetTutorialInfoData(_prevTutoID);
        UserData.Instance.LocalData.CurrTutorialID++;
        UserData.Instance.SaveLocalData();
        var nextTutoInfo = DataManager.Instance.GetTutorialInfoData(UserData.Instance.LocalData.CurrTutorialID);
        if (nextTutoInfo == null)
        {
            _hidePrevTuto?.Invoke();
            return;
        }

        switch (prevTutoInfo.tutotype)
        {
            case TUTO_TYPE.CUTSCENE:
                {
                    if (nextTutoInfo.tutotype != TUTO_TYPE.CUTSCENE)
                    {
                        _hidePrevTuto?.Invoke();
                    }
                    Play();
                }
                break;
            case TUTO_TYPE.DIALOUGE:
                {
                    if (nextTutoInfo.tutotype != TUTO_TYPE.DIALOUGE)
                    {
                        _hidePrevTuto?.Invoke();
                    }
                    Play();
                }
                break;
            case TUTO_TYPE.CAMERASTAGEMOVE:
                {
                    Play();
                }
                break;
            case TUTO_TYPE.NEEDTOUCH:
                {
                    MGameManager.Instance.HideTutorialTouchWait();
                    switch (prevTutoInfo.id)
                    {
                        case 7:
                            Play();
                            break;
                        case 8:
                            break;
                        case 9:
                            break;
                        default:
                            break;
                    }
                }
                break;
        }
    }

}
