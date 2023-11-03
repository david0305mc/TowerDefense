using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class SpawnTestManager : MonoBehaviour
{
    [SerializeField] private Button spawnButton;
    [SerializeField] private GameObject spawnTestItemPrefab;
    private GameObject spawnItem;

    private void Awake()
    {
        MessageDispather.Receive(EMessage.Update_HeroParty).Subscribe(_ =>
        {
            SpawnItem();
        }).AddTo(gameObject);

        spawnButton.onClick.AddListener(() =>
        {
            MessageDispather.Publish(EMessage.Update_HeroParty);
        });
    }

    private void SpawnItem()
    {
        if (spawnItem != null)
        {
            Lean.Pool.LeanPool.Despawn(spawnItem);
        }
        spawnItem = Lean.Pool.LeanPool.Spawn(spawnTestItemPrefab, Vector3.zero, Quaternion.identity, transform);
    }

}

