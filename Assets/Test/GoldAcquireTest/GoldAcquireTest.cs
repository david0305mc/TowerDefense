using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System;

public class GoldAcquireTest : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private GoldItemObj goldItemObjPrefab;
    // Start is called before the first frame update
    void Start()
    {
        Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                for (int i = 0; i < 1; i++)
                {
                    var randVec2 = new Vector2(UnityEngine.Random.Range(-3.0f, 3.0f), UnityEngine.Random.Range(-3.0f, 3.0f));
                    GoldItemObj itemObj = Lean.Pool.LeanPool.Spawn(goldItemObjPrefab, randVec2, Quaternion.identity, transform);
                    itemObj.Shoot(target, 1f);
                }

            }).AddTo(gameObject);
    }

}
