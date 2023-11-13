using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePopup : PopupBase
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button exitButton;

    public override void InitPopup(Action _hideAction)
    {
        base.InitPopup(()=> {
            Time.timeScale = UserData.Instance.GameSpeed;
            _hideAction?.Invoke();
        });
        Time.timeScale = 0f;
    }

    public void SetData(UnityEngine.Events.UnityAction _restartAction, UnityEngine.Events.UnityAction  _exitAction)
    {
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(_restartAction);

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(_exitAction);
    }

}
