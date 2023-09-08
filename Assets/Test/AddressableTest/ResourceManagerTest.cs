using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEditor;
using UnityEngine.U2D;

public class ResourceManagerTest : SingletonMono<ResourceManagerTest>
{

    [System.Serializable]
    public class AssetReferenceAtlas : AssetReferenceT<SpriteAtlas>
    {
        public AssetReferenceAtlas(string guid) : base(guid) { }
    }

    [SerializeField] private AssetReferenceAtlas testReferenceAtlas;
    private Dictionary<string, AsyncOperationHandle<GameObject>> OpHandleDic;
    private List<AsyncOperationHandle<SpriteAtlas>> opAtlasHandleLists;
    private AsyncOperationHandle<AtlasData> _opHandle;

    private SpriteAtlas spriteAtlas;
    
    async UniTaskVoid Start()
    {
        opAtlasHandleLists = new List<AsyncOperationHandle<SpriteAtlas>>();
        var obj = testReferenceAtlas.LoadAssetAsync();
        await obj;
        opAtlasHandleLists.Add(obj);
        //_opHandle = Addressables.LoadAssetAsync<AtlasData>("Atlas/AtlasData.asset");
        //_atlasData = _opHandle.WaitForCompletion();
        //if (_atlasData == null)
        //    return;

        //_atlas = new UIAtlas[_atlasData.atlas.Length];
        //for (int i = 0; i < _atlasData.atlas.Length; ++i)
        //{
        //    var go = await _atlasData.atlas[i].LoadAssetAsync().ToUniTask(cancellationToken: _cancelltaionTokenSource.Token);
        //    _atlas[i] = go.GetComponent<UIAtlas>();
        //}
    }

    public Sprite GetSpriteFromAtlas(string _name)
    {
        foreach (var item in opAtlasHandleLists)
        {
            Sprite sprite = item.Result.GetSprite(_name);
            if (sprite != null)
            {
                return sprite;
            }
        }
        return null;
    }
    public GameObject GetAddressableAtlas(string _name)
    {
        if (OpHandleDic.TryGetValue(_name, out var value))
        {
            return value.Result;
        }
        return null;
    }

    public async UniTaskVoid LoadAddressableAtlas(string _name)
    {
        OpHandleDic[_name] = Addressables.LoadAssetAsync<GameObject>(_name);
        await OpHandleDic[_name];
    }

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

    public async UniTaskVoid UnloadUnusedAssetsImmediate()
    {
#if UNITY_EDITOR
        await UniTask.Yield();
        EditorUtility.UnloadUnusedAssetsImmediate(true);
#endif
    }
}
