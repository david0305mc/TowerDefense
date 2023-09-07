using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SpawnTest : MonoBehaviour
{
    [SerializeField] private List<GameObject> stageprefLists;
    private Dictionary<string, GameObject> prefabDic;

    private int stage;
    private StageTest currStageObj;
    private Dictionary<string, HeroTest> heroTestDic;
    private List<EnemyTest> enemyLists;


    private void Start()
    {
        prefabDic = new Dictionary<string, GameObject>();
        heroTestDic = new Dictionary<string, HeroTest>();
        enemyLists = new List<EnemyTest>();
    }
    public void OnClickAddStageBtn()
    {
        if (currStageObj != null)
        {
            Destroy(currStageObj.gameObject);
        }
        currStageObj = Instantiate(stageprefLists[stage], Vector3.zero, Quaternion.identity, transform).GetComponent<StageTest>();

        var enemies = currStageObj.GetComponentsInChildren<EnemyTest>();
        foreach (EnemyTest enemyObj in enemies)
        {
            enemyLists.Add(enemyObj);
        }
    }

    public void OnClickAddHeroBtn()
    {
        for (int i = 1; i < 3; i++)
        {
            string name = $"Unit/HeroTest{i.ToString("D2")}.prefab";
            HeroTest heroObj = Lean.Pool.LeanPool.Spawn(prefabDic[name], Random.insideUnitCircle * 3, Quaternion.identity, transform).GetComponent<HeroTest>();
            heroTestDic.Add(name, heroObj);
        }
        
    }
    public void OnClickChageStageBtn()
    {
        stage++;
        if (currStageObj != null)
        {
            Destroy(currStageObj.gameObject);
        }
        currStageObj = Instantiate(stageprefLists[stage], Vector3.zero, Quaternion.identity, transform).GetComponent<StageTest>();
    }
    public void OnClickRemoveStageBtn()
    {
        Destroy(currStageObj.gameObject);
    }
    public void OnClickRemoveEnemyBtn()
    {
        for (int i = enemyLists.Count - 1; i >= 0; i--)
        {
            Destroy(enemyLists[i].gameObject);
        }
    }
    public void OnClickRemoveHeroBtn()
    {
        for (int i = 1; i < 3; i++)
        {
            string name = $"Unit/HeroTest{i.ToString("D2")}.prefab";
            Lean.Pool.LeanPool.Despawn(heroTestDic[name]);
            heroTestDic.Remove(name);
        }
    }

    public void OnClickLoadHeroAsset()
    {
        for (int i = 1; i < 3; i++)
        {
            string name = $"Unit/HeroTest{i.ToString("D2")}.prefab";
            prefabDic[name] = Addressables.LoadAssetAsync<GameObject>(name).WaitForCompletion();
            //var go = Addressables.InstantiateAsync($"Unit/AddressablePrefab0{i}.prefab", Vector3.zero, Quaternion.identity, transform);
        }
    }

    public void OnClickGoToIntro()
    {
        SceneManager.LoadScene("IntroTest");

    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.L))
    //    {
    //        //for (int i = 1; i < 6; i++)
    //        //{
    //        //    string name = $"Unit/AddressablePrefab0{i}.prefab";
    //        //    prefabDic[name] = Addressables.LoadAssetAsync<GameObject>(name).WaitForCompletion();
    //        //    //var go = Addressables.InstantiateAsync($"Unit/AddressablePrefab0{i}.prefab", Vector3.zero, Quaternion.identity, transform);
    //        //}

    //        for (int i = 1; i < 6; i++)
    //        {
    //            string name = $"Unit/Test{i.ToString("D2")}.prefab";
    //            //prefabDic[name] = Addressables.LoadAssetAsync<GameObject>(name).WaitForCompletion();
    //            var go = Addressables.InstantiateAsync(name, new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0), Quaternion.identity, transform);
    //        }
    //    }
    //    if (Input.GetKeyDown(KeyCode.O))
    //    {
    //        for (int i = 1; i < 6; i++)
    //        {
    //            string name = $"Unit/AddressablePrefab0{i}.prefab";
    //            LoadAsync(name);
    //        }
    //    }


    //    if (Input.GetKeyDown(KeyCode.P))
    //    {
    //        Stopwatch sw = new Stopwatch();
    //        sw.Start();
    //        for (int i = 0; i < 1000; i++)
    //        {
    //            foreach (var item in prefabDic)
    //            {
    //                Instantiate(item.Value, new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0), Quaternion.identity);
    //            }
    //        }
    //        sw.Stop();
    //        UnityEngine.Debug.Log($"Time : {sw.ElapsedMilliseconds}ms");
    //    }
    //}

    private async UniTask LoadAsync(string _name)
    {
        if (!prefabDic.ContainsKey(name))
        {
            prefabDic[_name] = await Addressables.LoadAssetAsync<GameObject>(_name);
        }
        Instantiate(prefabDic[_name], new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0), Quaternion.identity);
    }
}
