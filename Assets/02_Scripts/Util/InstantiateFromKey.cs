using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class InstantiateFromKey : MonoBehaviour
{
    public string key; // Identify the asset

    void Start()
    {
        // Load and instantiate
        Addressables.InstantiateAsync(key).Completed += instantiate_Completed;
    }

    private void instantiate_Completed(AsyncOperationHandle<GameObject> obj)
    {
        // Add component to release asset in GameObject OnDestroy event
        obj.Result.AddComponent(typeof(SelfCleanup));
    }
}

// Releases asset (trackHandle must be true in InstantiateAsync,
// which is the default)
public class SelfCleanup : MonoBehaviour
{
    void OnDestroy()
    {
        Addressables.ReleaseInstance(gameObject);
    }
}

//https://docs.unity3d.com/Packages/com.unity.addressables@1.18/manual/LoadingAddressableAssets.html