using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MMainUI : MonoBehaviour
{
    [SerializeField] private Button tankerBtn;
    [SerializeField] private Button archerBtn;
    [SerializeField] private Button nextStageBtn;
    private void Awake()
    {
        tankerBtn.onClick.AddListener(() =>
        {
            for (int i = 0; i < 1; i++)
            {
                MGameManager.Instance.AddHero(0);
            }
        });
        archerBtn.onClick.AddListener(() =>
        {
            for (int i = 0; i < 1; i++)
            {
                MGameManager.Instance.AddHero(1);
            }
        });
        nextStageBtn.onClick.AddListener(() =>
        {
            //MGameManager.Instance.NextStage();
            SceneManager.LoadSceneAsync("Intro");
        });
    }
}
