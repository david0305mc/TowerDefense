using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTest : MonoBehaviour
{
    [SerializeField] private GameObject spawnPrefab;

    private void Update()
    {
        if (Input.GetKey(KeyCode.T))
        {
            Instantiate(spawnPrefab, new Vector3(0, -10, 0), Quaternion.identity, transform);
        }
    }
}
