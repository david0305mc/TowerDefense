using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StageTest : MonoBehaviour
{
    private void OnDestroy()
    {
        Addressables.ReleaseInstance(gameObject);
    }
}
