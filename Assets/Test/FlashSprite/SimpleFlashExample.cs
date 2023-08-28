using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFlashExample : MonoBehaviour
{
    [SerializeField] private SimpleFlash flash;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            flash.Flash();
        }
    }
}
