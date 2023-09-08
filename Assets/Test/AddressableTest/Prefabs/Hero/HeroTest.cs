using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroTest : MonoBehaviour
{
    private System.Action destroyAction;
    public void SetDestroyAction(System.Action _action) 
    {
        destroyAction = _action;
    }

    private void OnDestroy()
    {
        Debug.Log("HeroTest destroy");
        destroyAction?.Invoke();
    }
}
