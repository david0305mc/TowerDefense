using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Linq;

public class ResourceManagerTest : SingletonMono<ResourceManagerTest>
{
    private Dictionary<string, AsyncOperationHandle<GameObject>> OpHandleDic;

    public GameObject GetAddressablePrefab(string _name)
    {
        if (OpHandleDic.TryGetValue(_name, out var value))
        {
            return value.Result;
        }
        return null;
    }

    public async UniTaskVoid LoadAddressable(string _name)
    {
        OpHandleDic[_name] = Addressables.LoadAssetAsync<GameObject>(_name);
        await OpHandleDic[_name];
    }
    public void UnloadAddressable()
    {
        for (int i = OpHandleDic.Count - 1; i >= 0; i--)
        {
            Addressables.Release(OpHandleDic.ElementAt(i).Value);
        }
        OpHandleDic.Clear();
    }

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();

        OpHandleDic = new Dictionary<string, AsyncOperationHandle<GameObject>>();
    }
}
