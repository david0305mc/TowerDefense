using System;
using UnityEngine;
using Game;

public class UnitTestManager : MonoBehaviour
{
    [SerializeField] private UnitTestObj[] unitObjLists = Array.Empty<UnitTestObj>();
    [SerializeField] private Transform objRoot;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (unitObjLists == null || !Array.Exists(unitObjLists, unit => unit != null))
        {
            unitObjLists = objRoot != null
                ? objRoot.GetComponentsInChildren<UnitTestObj>()
                : FindObjectsOfType<UnitTestObj>();
        }

        if (unitObjLists.Length == 0)
            Debug.LogWarning("UnitTestManager: no test units found. Add UnitTestObj objects to this scene or assign Unit Obj Lists.", this);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)
            && TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask, out Vector3 groundHitPoint))
        {
            MoveUnits(groundHitPoint);
        }

        if (Input.GetMouseButtonDown(1))
            AttackUnits();
    }

    private void MoveUnits(Vector3 target)
    {
        foreach (var unit in unitObjLists ?? Array.Empty<UnitTestObj>())
        {
            // Scene edits or runtime destruction can leave missing entries in the list.
            if (unit != null)
                unit.SetTargetPos(target);
        }
    }

    private void AttackUnits()
    {
        foreach (var unit in unitObjLists ?? Array.Empty<UnitTestObj>())
        {
            if (unit != null)
                unit.PlayAni("Attack");
        }
    }

    private bool TryGetRayCastHitPoint(Vector2 touchPoint, int layerMask, out Vector3 point)
    {
        point = default;
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null)
            return false;

        RaycastHit2D hit = Physics2D.GetRayIntersection(mainCamera.ScreenPointToRay(touchPoint), Mathf.Infinity, layerMask);
        if (!hit)
            return false;

        point = hit.point;
        return true;
    }
}
