using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissileTest : MonoBehaviour
{
    [SerializeField] private GameObject srcObj;
    [SerializeField] private GameObject dstObj;
    [SerializeField] private TestMissileObj01 missile01;
    [SerializeField] private GravityMissileTest gravityMissile;

    [SerializeField] private Button button;

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            Shoot();
        });
    }

    private void Shoot()
    {
        //var missile = Lean.Pool.LeanPool.Spawn(missile01, srcObj.transform);
        //missile.Shoot(dstObj, 3);
        var missile = Lean.Pool.LeanPool.Spawn(gravityMissile, srcObj.transform);
        missile.Shoot(dstObj.transform.position, 2, -20);
    }


}
