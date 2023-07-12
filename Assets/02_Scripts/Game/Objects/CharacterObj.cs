using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class CharacterObj : BaseObj
{
    private CancellationTokenSource cts;

    private float speed = 5;
    public void MoveToPostion(Vector3 targetPosition)
    {
        cts?.Clear();
        cts = new CancellationTokenSource();
        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.Update).ForEachAsync(_ =>
        {
            float frameDistance = Time.deltaTime * speed;
            float interpolationValue = frameDistance / (targetPosition - transform.localPosition).magnitude;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, interpolationValue);
        }, cts.Token);

    }
}
