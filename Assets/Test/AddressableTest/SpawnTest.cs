using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SpawnTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKey(KeyCode.T))
        {
            //Instantiate(spawnPrefab, new Vector3(0, -10, 0), Quaternion.identity, transform);
            for (int i = 1; i < 5; i++)
            {
                var go = Addressables.InstantiateAsync($"AddressablePrefab0{i}.prefab", Vector3.zero, Quaternion.identity, transform);
            }
            
        }

    }
}
