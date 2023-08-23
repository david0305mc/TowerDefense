using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.EventSystems;
using Game;

public class MCameraManager : MonoBehaviour
{
    [SerializeField] private int mapSizeMinX = -20;
    [SerializeField] private int mapSizeMaxX = 20;
    [SerializeField] private int mapSizeMinY = -10;
    [SerializeField] private int mapSizeMaxY = 30;

    [SerializeField] private float dragSpeed = 3f;

    private static Vector3 PositiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    private Camera mainCamera;

    private Vector3 newPos;
    private Vector3 oldPos;

    private Vector3 dragStartPos = Vector3.zero;
    private bool groundDragStarted = false;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        newPos = transform.position;
        oldPos = newPos;
    }

    void Start()
    {
        dragStartPos = Vector3.zero;
        groundDragStarted = false;
    }
    
    private void Update()
    {
        UpdateOneTouch();
    }

    private void FixedUpdate()
    {
        newPos = new Vector3(Mathf.Clamp(newPos.x, mapSizeMinX, mapSizeMaxX), Mathf.Clamp(newPos.y, mapSizeMinY, mapSizeMaxY), -10);
        if (!newPos.Equals(oldPos))
        {
            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * dragSpeed);
            oldPos = transform.position;
        }
    }
    private void UpdateOneTouch()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 groudHitPoint = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
            if (!groudHitPoint.Equals(PositiveInfinityVector))
            {
                dragStartPos = groudHitPoint;
                groundDragStarted = true;
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (groundDragStarted)
            {
                Vector3 hitPoint = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
                if (!hitPoint.Equals(PositiveInfinityVector))
                {
                    newPos = transform.position + dragStartPos - hitPoint;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            groundDragStarted = false;
            dragStartPos = PositiveInfinityVector;
        }
    }

    public bool IsUsingUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
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
            return PositiveInfinityVector;
        }
    }
}
