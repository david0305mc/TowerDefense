using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectTestManager : MonoBehaviour
{
    [SerializeField] private CartoonFX.CFXR_Effect effectPref;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var effect = Lean.Pool.LeanPool.Spawn(effectPref);
            effect.EndAction = () =>{
                Lean.Pool.LeanPool.Despawn(effect);
            };
            effect.transform.position = mousePosition;
        }
    }
}
