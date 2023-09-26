using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameResultPopup : PopupBase
{
    [SerializeField] private Button homeBtn;
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button nextStageBtn;
    [SerializeField] private GameObject winObject;
    [SerializeField] private GameObject loseObject;

    protected override void Awake()
    {
        base.Awake();
    }

    public void SetData(bool isWin, System.Action _homeAction, System.Action _retryAction, System.Action _nextStageAction)
    {
        if (isWin)
        {
            winObject.SetActive(true);
            loseObject.SetActive(false);
        }
        else
        {
            winObject.SetActive(false);
            loseObject.SetActive(true);
        }

        homeBtn.onClick.RemoveAllListeners();
        homeBtn.onClick.AddListener(() => {
            Hide();
            _homeAction?.Invoke();
        });

        retryBtn.onClick.RemoveAllListeners();
        retryBtn.onClick.AddListener(() => {
            Hide();
            _retryAction?.Invoke();
        });

        nextStageBtn.onClick.RemoveAllListeners();
        nextStageBtn.onClick.AddListener(() => {
            Hide();
            _nextStageAction?.Invoke();
        });
    }
}
