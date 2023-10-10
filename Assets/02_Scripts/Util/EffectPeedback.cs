using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class EffectPeedback : MonoBehaviour
{
    private ParticleSystem particle;
    private AnimationLink animationLink;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        animationLink = GetComponentInChildren<AnimationLink>();
    }

    public void SetData(System.Action _endAction, CancellationTokenSource _cts)
    {
        if (animationLink != null)
        {
            animationLink.SetEvent(null, () =>
            {
                _endAction?.Invoke();
            });
        }
        else
        {
            UniTask.Create(async () =>
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(particle.main.duration), cancellationToken: _cts.Token);
                _endAction?.Invoke();
            });
        }
    }

}
