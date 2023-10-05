using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UIIntro : MonoBehaviour
{
   
    [SerializeField] private Button startBtn;
    [SerializeField] private List<GameObject> preloadingObjLists;
    [SerializeField] private List<GameObject> loadingObjectLists;

    private void Awake()
    {
        Application.targetFrameRate = 120;
        
        startBtn.onClick.AddListener(() =>
        {
            foreach (var item in preloadingObjLists)
            {
                item.SetActive(false);
            }
            foreach (var item in loadingObjectLists)
            {
                item.SetActive(true);
            }
            StartGame().Forget();
        });
        foreach (var item in preloadingObjLists)
        {
            item.SetActive(true);
        }
        foreach (var item in loadingObjectLists)
        {
            item.SetActive(false);
        }
    }

    private async UniTaskVoid StartGame()
    {
        await UniTask.Yield();
        await UniTask.Yield();
        LocalizeManager.Instance.Initialize();
        await Resources.UnloadUnusedAssets();
        await DataManager.Instance.LoadDataAsync();
        await DataManager.Instance.LoadConfigTable();
        await MResourceManager.Instance.LoadResources();
        DataManager.Instance.MakeClientDT();
        UserData.Instance.InitData();
        UserData.Instance.LoadLocalData();
        UserData.Instance.UpdateData();
        GameTime.InitLocalBase();

        var mainSceneAsync = SceneManager.LoadSceneAsync("Main");
        await mainSceneAsync;
        //SceneManager.LoadScene("Main");
    }
}
