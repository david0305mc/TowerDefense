using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationLink : MonoBehaviour
{
    System.Action action;
    public void SetFireEvent(System.Action _action)
    {
        action = _action;
    }
    public void Fire()
    {
        action?.Invoke();
    }
}
