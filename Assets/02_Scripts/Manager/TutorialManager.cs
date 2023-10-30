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
        switch (tutoInfo.tutotype)
        {
            case TUTO_TYPE.CUTSCENE:
                {
                    var popup = PopupManager.Instance.Show<CutScenePopup>();
                    popup.SetData(int.Parse(tutoInfo.value1), () => 
                    { 
                        PlayEnd(tutoInfo.tutotype); 
                    });
                }
                break;
            case TUTO_TYPE.DIALOUGE:
                break;
            case TUTO_TYPE.CAMERASTAGEMOVE:
                break;
            case TUTO_TYPE.NEEDTOUCH:
                break;
        }
    }

    public void PlayEnd(TUTO_TYPE _prevTutoType)
    {
        UserData.Instance.LocalData.CurrTutorialID++;
        UserData.Instance.SaveLocalData();

        var nextTutoInfo = DataManager.Instance.GetTutorialInfoData(UserData.Instance.LocalData.CurrTutorialID);
        switch (_prevTutoType)
        {
            case TUTO_TYPE.CUTSCENE:
                {
                    if (nextTutoInfo.tutotype != TUTO_TYPE.CUTSCENE)
                    {
                        PopupManager.Instance.Hide<CutScenePopup>();
                    }
                }
                break;
            case TUTO_TYPE.DIALOUGE:
                break;
            case TUTO_TYPE.CAMERASTAGEMOVE:
                break;
            case TUTO_TYPE.NEEDTOUCH:
                break;
        }
        Play();
    }

}
