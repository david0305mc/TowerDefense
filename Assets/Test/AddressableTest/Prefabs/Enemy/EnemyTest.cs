using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class EnemyTest : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnDestroy()
    {
        Addressables.ReleaseInstance(gameObject);
    }
}
