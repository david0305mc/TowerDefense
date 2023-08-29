using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CodeTest : MonoBehaviour
{
    private CancellationTokenSource AAACancellation;


    private void Start()
    {
        AAA().Forget();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AAACancellation?.Cancel();
        }
    }
    public async UniTask AAA()
    {
        var i = 0;
        AAACancellation?.Cancel();
        AAACancellation = new CancellationTokenSource();
        //var link = CancellationTokenSource.CreateLinkedTokenSource(disableCancellation.Token, AAACancellation.Token);
        while (i < 20)
        {
            Debug.Log("AAA : " + i++);
            await UniTask.Delay(1000, cancellationToken: AAACancellation.Token);
        }
        Debug.Log("AAA ¿Ï·á !");
    }
}
