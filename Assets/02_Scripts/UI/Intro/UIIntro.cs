using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UIIntro : MonoBehaviour
{
   [SerializeField] private Button startBtn;

    private void Awake()
    {
        Application.targetFrameRate = 120;
        
        startBtn.onClick.AddListener(() =>
        {
            startBtn.enabled = false;
            StartGame().Forget();
        });
    }
    private void OnEnable()
    {
        startBtn.enabled = true;

    }

    private async UniTaskVoid StartGame()
    {
        LocalizeManager.Instance.Initialize();
        await Resources.UnloadUnusedAssets();
        await DataManager.Instance.LoadDataAsync();
        await DataManager.Instance.LoadConfigTable();
        await MResourceManager.Instance.LoadResources();
        DataManager.Instance.MakeClientDT();
        UserData.Instance.InitData();
        UserData.Instance.LoadLocalData();

        var mainSceneAsync = SceneManager.LoadSceneAsync("Main");
        await mainSceneAsync;
        //SceneManager.LoadScene("Main");
    }
}
