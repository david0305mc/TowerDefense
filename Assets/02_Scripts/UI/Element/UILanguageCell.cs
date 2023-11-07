using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class UILanguageCell : MonoBehaviour
{
    [SerializeField] private LANGUAGE_TYPE languageType;
    [SerializeField] private Image bgImage;
    [SerializeField] private Button button;
    [SerializeField] private SpriteAtlas commonAtlas;

    public void SetData(System.Action<LANGUAGE_TYPE> _action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            _action?.Invoke(languageType);
        });

        if (LocalizeManager.Instance.Language == languageType)
        {
            bgImage.sprite = commonAtlas.GetSprite("Popup_Sub_BTN_On");
        }
        else
        {
            bgImage.sprite = commonAtlas.GetSprite("Popup_Sub_BTN_Off");
        }
    }

}
