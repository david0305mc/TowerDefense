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
        property.ThrottleFrame(0).Subscribe(val => Debug.Log(val));
        property.Value = 1;
        property.Value = 2;
        property.Value = 3;

        await UniTask.NextFrame();

        property.Value = 4;
        property.Value = 5;
        await UniTask.NextFrame();
        property.Value = 6;
        property.Value = 7;
        await UniTask.NextFrame();
        property.Value = 8;
        property.Value = 9;
        await UniTask.NextFrame();
        property.Value = 10;
        property.Value = 11;
    }

}
