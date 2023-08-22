using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationLink : MonoBehaviour
{
    System.Action fireAction;
    System.Action aniEndAction;
    private Animator animator;
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

    private void Awake()
    {
        animator = GetComponent<Animator>();
        for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++)
        {
            AnimationClip clip = animator.runtimeAnimatorController.animationClips[i];

            AnimationEvent animationEndEvent = new AnimationEvent();
            animationEndEvent.time = clip.length;
            animationEndEvent.functionName = "AniEnd";
            animationEndEvent.stringParameter = clip.name;
            clip.AddEvent(animationEndEvent);
        }
    }
}
