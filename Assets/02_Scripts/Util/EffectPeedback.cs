using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class EffectPeedback : MonoBehaviour
{
    private ParticleSystem particle;
    private void Awake()
    {
        particle = GetComponent<ParticleSystem>(); 
    }

    public void SetData(System.Action _endAction, CancellationTokenSource _cts)
    {
        UniTask.Create(async () =>
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(particle.main.duration), cancellationToken:_cts.Token);
            _endAction?.Invoke();
        });
    }

}
