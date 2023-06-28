using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float panFactor = 0.05f;
    [SerializeField] private float panReduceFactor = 10f;
    private Vector3 tabGroundStartPosition;
    private int previousTouchCount;
    private Vector3 previoursPanPoint;
    private Vector3 panVelocity;
    private bool isPanningStarted;

    void Update()
    {
        UpdatePan();    
    }


    private void UpdatePan()
    {
        int touchCount = Input.touchCount;
        bool isInEditor = false;
        bool touchCountChanged = false;
        bool canPan;
        Vector2 touchPosition;

        if (Input.touchCount == 0)
        {
            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)))
            {
                touchCount = 1;
                isInEditor = true;
            }
            else
            {
                touchCount = 0;
            }
        }
        else
        {
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                touchCount = 0;
            }
            else
            {
                touchCount = Input.touchCount;
            }
        }

        if (touchCount != previousTouchCount)
        {
            if (touchCount > 0)
            {
                touchCountChanged = true;
            }
        }

        if (isInEditor)
        {
            touchPosition = Input.mousePosition;
        }
        else
        {
            if (touchCount > 0)
            {
                if (touchCount == 1)
                {
                    touchPosition = Input.GetTouch(0).position;
                }
                else
                {
                    touchPosition = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2.0f;
                }
            }
            else
            {
                touchPosition = Vector2.zero;
            }
        }

        canPan = touchCount > 0;
        previousTouchCount = touchCount;

        if (canPan)
        {
            Vector3 hitPoint = TryGetRayCastHit(touchPosition, GameConfig.GroundLayerMask);

            if (touchCountChanged)
            {
                tabGroundStartPosition = hitPoint;
                previoursPanPoint = hitPoint;
            }

            if (!isPanningStarted && (tabGroundStartPosition - hitPoint).magnitude > 2f)
            {
                isPanningStarted = true;
                previoursPanPoint = hitPoint;
            }

            if (isPanningStarted)
            {
                OnScenePan(hitPoint);
            }
        }
        else
        {
            if (isPanningStarted)
            {
                isPanningStarted = false;
                OnScenePanEnded();
            }
            UpdatePanInertia();
        }
    }

    private void OnScenePanEnded()
    {
        Debug.Log($"stop _panVelocity{panVelocity}");
    }
    private void OnScenePan(Vector3 newPoint)
    {
        Vector3 delta = previoursPanPoint - newPoint;
        Debug.Log($"_previous {previoursPanPoint} evtpoint {newPoint} delta {delta}");
        mainCamera.transform.localPosition += delta;
        panVelocity = delta;
        ClampCamera();
    }

    private void UpdatePanInertia()
    {
        if (panVelocity.magnitude < panFactor) 
        {
            panVelocity = Vector3.zero;
        }

        if (panVelocity != Vector3.zero)
        {
            panVelocity = Vector3.Lerp(panVelocity, Vector3.zero, Time.deltaTime * panReduceFactor);
            mainCamera.transform.localPosition += panVelocity;
            ClampCamera();
        }
    }

    private void ClampCamera()
    { 
    
    }


    private Vector3 TryGetRayCastHit(Vector2 touchPoint, int layerMask)
    {
        RaycastHit hit;
        var ray = mainCamera.ScreenPointToRay(touchPoint);
        if (Physics.Raycast(ray, out hit, 1000, layerMask))
        {
            return hit.point;
        }
        else
        {
            return Vector3.positiveInfinity;
        }            
    }
}
