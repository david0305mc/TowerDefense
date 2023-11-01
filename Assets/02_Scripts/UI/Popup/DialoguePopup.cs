using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class DialoguePopup : PopupBase
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button nextButton;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void InitPopup(Action _hideAction)
    {
        base.InitPopup(_hideAction);
    }

    public void SetData(int _tutoID, UnityEngine.Events.UnityAction _action)
    {
        var tutoInfo = DataManager.Instance.GetTutorialInfoData(_tutoID);
        var dialogueInfo = DataManager.Instance.GetDialogueData(int.Parse(tutoInfo.value1));
        messageText.SetText(LocalizeManager.Instance.GetLocalString(dialogueInfo.localizekey));

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(_action);
        nextButton.enabled = false;
        UniTask.Create(async () =>
        {
            await UniTask.Delay(tutoInfo.delay);
            nextButton.enabled = true;
        });
    }

}
