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
    [SerializeField] private LoadingUI loadingUI;

    private void Awake()
    {
        Application.targetFrameRate = 120;
        
        startBtn.onClick.AddListener(() =>
        {
            foreach (var item in preloadingObjLists)
            {
                item.SetActive(false);
            }
            StartGame().Forget();
        });
        foreach (var item in preloadingObjLists)
        {
            item.SetActive(true);
        }
        loadingUI.SetActive(false);
    }

    private async UniTaskVoid StartGame()
    {
        loadingUI.SetActive(true); 
        var playLoadingUI = loadingUI.PlayLoadingUIAsync();
        await UniTask.Yield();
        await UniTask.Yield();
        LocalizeManager.Instance.Initialize();
        await Resources.UnloadUnusedAssets();
        await DataManager.Instance.LoadDataAsync();
        await DataManager.Instance.LoadConfigTable();
        await MResourceManager.Instance.LoadResources();
        await playLoadingUI;
        SoundManager.Instance.Play("Bgm/Bgm_01", SoundType.Bgm);
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
