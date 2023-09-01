using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Diagnostics;
using Cysharp.Threading.Tasks;

public class SpawnTest : MonoBehaviour
{

    private Dictionary<string, GameObject> prefabDic = new Dictionary<string, GameObject>();
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            //for (int i = 1; i < 6; i++)
            //{
            //    string name = $"Unit/AddressablePrefab0{i}.prefab";
            //    prefabDic[name] = Addressables.LoadAssetAsync<GameObject>(name).WaitForCompletion();
            //    //var go = Addressables.InstantiateAsync($"Unit/AddressablePrefab0{i}.prefab", Vector3.zero, Quaternion.identity, transform);
            //}

            for (int j = 0; j < 1000; j++)
            {
                for (int i = 1; i < 6; i++)
                {
                    string name = $"Unit/AddressablePrefab0{i}.prefab";
                    //prefabDic[name] = Addressables.LoadAssetAsync<GameObject>(name).WaitForCompletion();
                    var go = Addressables.InstantiateAsync(name, new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0), Quaternion.identity, transform);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            for (int i = 1; i < 6; i++)
            {
                string name = $"Unit/AddressablePrefab0{i}.prefab";
                prefabDic[name] = Addressables.LoadAssetAsync<GameObject>(name).WaitForCompletion();
                //var go = Addressables.InstantiateAsync($"Unit/AddressablePrefab0{i}.prefab", Vector3.zero, Quaternion.identity, transform);
            }
        }


        if (Input.GetKeyDown(KeyCode.P))
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000; i++)
            {
                foreach (var item in prefabDic)
                {
                    Instantiate(item.Value, new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0), Quaternion.identity);
                }
            }
            sw.Stop();
            UnityEngine.Debug.Log($"Time : {sw.ElapsedMilliseconds}ms");
        }

    }
}
