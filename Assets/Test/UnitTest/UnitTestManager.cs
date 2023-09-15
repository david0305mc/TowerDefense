using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;

public class UnitTestManager : MonoBehaviour
{
    [SerializeField] private UnitTestObj[] unitObjLists;
    //[SerializeField] private UnitTestObj unitObj;
    [SerializeField] private Transform objRoot;

    private Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 groudHitPoint = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
            Debug.Log($"groudHitPoint {groudHitPoint}");
            //Lean.Pool.LeanPool.Spawn(unitObj, groudHitPoint, Quaternion.identity, objRoot);

            for (int i = 0; i < unitObjLists.Length; i++)
            {
                
                unitObjLists[i].SetTargetPos(groudHitPoint);
            }

            //unitObj.SetTargetPos(groudHitPoint);
        }
        if (Input.GetMouseButtonDown(1))
        {

            for (int i = 0; i < unitObjLists.Length; i++)
            {
                unitObjLists[i].PlayAni("Attack");
            }
        }
    }


        private Vector3 TryGetRayCastHitPoint(Vector2 _touchPoint, int _layerMask)
    {
        var mousePos = mainCamera.ScreenToWorldPoint(_touchPoint);

        RaycastHit2D hit = Physics2D.Raycast(mousePos, transform.forward, 15f, layerMask: _layerMask);
        if (hit)
        {
            return hit.point;
        }
        else
        {
            return GameConfig.PositiveInfinityVector;
        }
    }
}
