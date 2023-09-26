using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnTestManager : MonoBehaviour
{
    [SerializeField] private Button spawnButton;
    [SerializeField] private GameObject spawnTestItemPrefab;
    private GameObject spawnItem;

    private void Awake()
    {
        spawnButton.onClick.AddListener(() => {

            if (spawnItem != null)
            {
                Lean.Pool.LeanPool.Despawn(spawnItem);
            }
            spawnItem = Lean.Pool.LeanPool.Spawn(spawnTestItemPrefab, Vector3.zero, Quaternion.identity, transform); 
        });
    }

}

