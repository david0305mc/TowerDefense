using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class CutScenePopup : PopupBase
{
    [SerializeField] private GameObject curScene01;
    [SerializeField] private GameObject curScene02;
    [SerializeField] private GameObject curScene03;
    [SerializeField] private GameObject curScene04;
    [SerializeField] private Button button;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void InitPopup(Action _hideAction)
    {
        base.InitPopup(_hideAction);
    }

    public void SetData(int _cutSceneID, int _delay, UnityEngine.Events.UnityAction _action)
    {
        curScene01.SetActive(_cutSceneID >= 1);
        curScene02.SetActive(_cutSceneID >= 2);
        curScene03.SetActive(_cutSceneID >= 3);
        curScene04.SetActive(_cutSceneID >= 4);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(_action);
        button.enabled = false;

        UniTask.Create(async () =>
        {
            await UniTask.Delay(_delay);
            button.enabled = true;
        });
    }

}

