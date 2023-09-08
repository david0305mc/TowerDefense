using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEditor;

public class SpawnTest : MonoBehaviour
{

    [SerializeField] private List<AssetReferenceGameObject> stageAssetReference;
    [SerializeField] private AssetReferenceGameObject testAssetReference;

    private int stage;
    private StageTest currStageObj;
    private List<HeroTest> heroLists;
    private List<EnemyTest> enemyLists;
    private GameObject testObj;


    private void Start()
    {
        heroLists = new List<HeroTest>();
    }

    private async UniTask InstantiateStage()
    {
        var result = await stageAssetReference[stage].InstantiateAsync(Vector3.zero, Quaternion.identity, transform);
        currStageObj = result.GetComponent<StageTest>();

        enemyLists = new List<EnemyTest>();
        var enemies = currStageObj.GetComponentsInChildren<EnemyTest>();
        foreach (EnemyTest enemyObj in enemies)
        {
            enemyLists.Add(enemyObj);
        }
    }

    public void OnClickAddStageBtn()
    {
        if (currStageObj != null)
        {
            Destroy(currStageObj.gameObject);
        }
        InstantiateStage().Forget();
    }

    public void OnClickAddHeroBtn()
    {
        for (int i = 1; i < 4; i++)
        {
            string name = $"Hero/HeroTest{i.ToString("D2")}.prefab";
            HeroTest heroObj = Lean.Pool.LeanPool.Spawn(ResourceManagerTest.Instance.GetAddressablePrefab(name), Random.insideUnitCircle * 3, Quaternion.identity, transform).GetComponent<HeroTest>();
            heroObj.SetDestroyAction(() =>
            {
                // Destroy
            });
            heroLists.Add(heroObj);
        }
        
    }
    public void OnClickChageStageBtn()
    {
        stage++;
        if (currStageObj != null)
        {
            Destroy(currStageObj.gameObject);
        }
        InstantiateStage().Forget();
        ResourceManagerTest.Instance.UnloadUnusedAssetsImmediate().Forget();
    }
    public void OnClickRemoveStageBtn()
    {
        if (!Addressables.ReleaseInstance(currStageObj.gameObject))
        {
            Destroy(currStageObj.gameObject);
        }
        ResourceManagerTest.Instance.UnloadUnusedAssetsImmediate().Forget();
    }
    public void OnClickRemoveEnemyBtn()
    {
        for (int i = enemyLists.Count - 1; i >= 0; i--)
        {
            Destroy(enemyLists[i].gameObject);
        }
        ResourceManagerTest.Instance.UnloadUnusedAssetsImmediate().Forget();
    }
    public void OnClickRemoveHeroBtn()
    {
        for (int i = heroLists.Count - 1; i >= 0; i--)
        {
            string name = $"Hero/HeroTest{i.ToString("D2")}.prefab";
            Lean.Pool.LeanPool.Despawn(heroLists[i]);
            heroLists.RemoveAt(i);
        }

        var poolLists = Lean.Pool.LeanGameObjectPool.Instances.ToList();
        for (int i = poolLists.Count - 1; i >= 0; i--)
        {
            Destroy(poolLists[i].gameObject);
        }
        
        ResourceManagerTest.Instance.UnloadAddressable();
        ResourceManagerTest.Instance.UnloadUnusedAssetsImmediate().Forget();
    }

    public void OnClickLoadHeroAsset()
    {
        UniTask.Create(async () =>
        {
            for (int i = 1; i < 4; i++)
            {

                string name = $"Hero/HeroTest{i.ToString("D2")}.prefab";
                ResourceManagerTest.Instance.LoadAddressable(name).Forget();
                //prefabDic[name] = Addressables.LoadAssetAsync<GameObject>(name).WaitForCompletion();
                //var go = Addressables.InstantiateAsync($"Unit/AddressablePrefab0{i}.prefab", Vector3.zero, Quaternion.identity, transform);
            }
        });
        
    }

    public void OnClickGoToIntro()
    {
        OnClickRemoveHeroBtn();
        SceneManager.LoadScene("IntroTest");
        ResourceManagerTest.Instance.UnloadAddressable();
    }


    public void InstantiateTestReference()
    {
        //Addressables.InstantiateAsync("Objects/Obj01.prefab").Completed += handle => {
        //    testObj = handle.Result;
        //    testObj.AddComponent(typeof(SelfCleanup));
        //};

        UniTask.Create(async () =>
        {
            var obj = Addressables.InstantiateAsync(testAssetReference, Vector3.zero, Quaternion.identity, transform);
            testObj = await obj;
            testObj.AddComponent(typeof(SelfCleanup));
        });

    }
    public void RelseaseTestReference()
    {
        //Destroy(testObj);

        //Destroy(testObj);
        //Addressables.Release(testObj);

        Destroy(testObj);

        //if (!Addressables.ReleaseInstance(testObj))
        //{
        //    Destroy(testObj);
        //}
        ResourceManagerTest.Instance.UnloadUnusedAssetsImmediate().Forget();
    }

}
