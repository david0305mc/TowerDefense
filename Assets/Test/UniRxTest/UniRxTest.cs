using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using Cysharp.Threading.Tasks;

public class UniRxTest : MonoBehaviour
{
    IntReactiveProperty property = new IntReactiveProperty();

    private void Start()
    {
        Test03().Forget();
    }
    async UniTask Test01()
    {
        property.ThrottleFirst(TimeSpan.FromSeconds(1f)).Subscribe(val => Debug.Log(val));

        await UniTask.WaitForSeconds(0.5f);
        property.Value = 1;
        await UniTask.WaitForSeconds(1f);
        property.Value = 2;
    }
    async UniTask Test02()
    {
        property.Throttle(TimeSpan.FromSeconds(1f)).Subscribe(val => Debug.Log(val));

        await UniTask.WaitForSeconds(0.5f);
        property.Value = 1;
        await UniTask.WaitForSeconds(1f);
        property.Value = 2;
    }
    async UniTask Test03()
    {
        property.ThrottleFrame(1).Subscribe(val => Debug.Log(val));
        
        await UniTask.Yield();
    }

}
