using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MMainUI : MonoBehaviour
{
    [SerializeField] private Button test01Btn;
    [SerializeField] private Button nextStageBtn;
    private void Awake()
    {
        test01Btn.onClick.AddListener(() =>
        {
            MGameManager.Instance.AddHero();
        });
        nextStageBtn.onClick.AddListener(() =>
        {
            MGameManager.Instance.NextStage();
        });
    }
}
