using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AnimationLink : MonoBehaviour
{
    System.Action fireAction;
    System.Action aniEndAction;
    System.Action<float> timeScaleAction;
    private Animator animator;
 

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            return;

        for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++)
        {
            AnimationClip clip = animator.runtimeAnimatorController.animationClips[i];
            if (System.Array.Find(clip.events, item => item.functionName == "AniEnd") == default)
            {
                AnimationEvent animationEndEvent = new AnimationEvent();
                animationEndEvent.time = clip.length;
                animationEndEvent.functionName = "AniEnd";
                animationEndEvent.stringParameter = clip.name;
                clip.AddEvent(animationEndEvent);
            }
        }
    }
    public void SetEvent(System.Action _fireAction, System.Action _aniEndAction)
    {
        fireAction = _fireAction;
        aniEndAction = _aniEndAction;
    }
    public void Fire()
    {
        fireAction?.Invoke();
    }
    public void AniEnd()
    {
        aniEndAction?.Invoke();
    }

    public void SetTimeScaleAction(System.Action<float> _actoin)
    {
        timeScaleAction = _actoin;
    }

    public void TimeScale(float _scale)
    {
        timeScaleAction?.Invoke(_scale);
    }
}
