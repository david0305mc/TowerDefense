using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.EventSystems;
using Game;

public class MCameraManager : SingletonMono<MCameraManager>
{
    [SerializeField] private int mapSizeMinX = -20;
    [SerializeField] private int mapSizeMaxX = 20;
    [SerializeField] private int mapSizeMinY = -10;
    [SerializeField] private int mapSizeMaxY = 30;

    [SerializeField] private float dragSpeed = 3f;

    [SerializeField] private float maxZoomFactor = 50;
    [SerializeField] private float minZoomFactor = 3;
    [SerializeField] private float zoomSpeed = 10f;


    private static Vector3 PositiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    private Camera mainCamera;

    private Vector3 newPos;
    private Vector3 oldPos;

    private float newZoom;
    private float oldZoom;

    bool pinchStarted = false;
    float oldPinchDist = 0f;

    private Vector3 dragStartPos = Vector3.zero;
    private bool groundDragStarted = false;

    private System.Action touchAction;
    private System.Action dragStartAction;
    private GameObject followTarget;
    private System.Action followTargetAction;

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();
        mainCamera = GetComponent<Camera>();
        newPos = transform.position;
        oldPos = newPos;
        newZoom = mainCamera.orthographicSize;
        oldZoom = newZoom;
        followTarget = null;
        followTargetAction = null;
    }
    void Start()
    {
        dragStartPos = Vector3.zero;
        groundDragStarted = false;
    }

    private void Update()
    {
        if (IsUsingUI())
        {
            return;
        }
        
        UpdateOneTouch();
#if UNITY_EDITOR
        UpdateEditorInput();
#else
        UpdateTwoTouch();
        CheckTouchRelesase();
#endif

    }

    private void FixedUpdate()
    {
        newPos = new Vector3(Mathf.Clamp(newPos.x, mapSizeMinX, mapSizeMaxX), Mathf.Clamp(newPos.y, mapSizeMinY, mapSizeMaxY), -10);
        if (!newPos.Equals(oldPos))
        {
            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * dragSpeed);
            oldPos = transform.position;
        }

        newZoom = Mathf.Clamp(newZoom, minZoomFactor, maxZoomFactor);
        if (!newZoom.Equals(oldZoom))
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, newZoom, Time.deltaTime * zoomSpeed);
            oldZoom = mainCamera.orthographicSize;
        }

        if (followTarget != null)
        {
            if (Vector2.Distance(transform.position, followTarget.transform.position) <= 0.1f)
            {
                followTargetAction?.Invoke();
                followTarget = null;
                followTargetAction = null;
            }
        }
    }
    void UpdateEditorInput()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 touchPoint0 = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
                var touchPoint1 = new Vector3(480, 960, 0);
                float pinchDist = Vector3.Distance(touchPoint0, touchPoint1);

                if (!pinchStarted)
                {
                    oldPinchDist = pinchDist;
                    pinchStarted = true;
                }
                else
                {
                    float delta = oldPinchDist - pinchDist;
                    newZoom = mainCamera.orthographicSize + delta / 2;
                }
            }
            else
            {
                pinchStarted = false;
            }
        }
        else
        {
            pinchStarted = false;
            float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * 10;
            if (scrollAmount != 0)
            {
                newZoom = newZoom - scrollAmount;
            }
        }
    }

    private void UpdateOneTouch()
    {
        if (Input.touchCount > 1)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 groudHitPoint = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
            if (!groudHitPoint.Equals(PositiveInfinityVector))
            {
                dragStartPos = groudHitPoint;
                groundDragStarted = true;
                dragStartAction?.Invoke();
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
            Vector3 groudHitPoint = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
            if (Vector3.Distance(dragStartPos, groudHitPoint) <= 0.1f)
            {
                touchAction?.Invoke();
            }
            groundDragStarted = false;
            dragStartPos = PositiveInfinityVector;
        }
    }

    private void UpdateTwoTouch()
    {
        if (Input.touchCount == 0 || Input.touchCount == 1)
        {
            pinchStarted = false;
        }

        if (Input.touchCount == 2)
        {
            var touchPoint0 = TryGetRayCastHitPoint(Input.GetTouch(0).position, GameConfig.GroundLayerMask);
            var touchPoint1 = TryGetRayCastHitPoint(Input.GetTouch(1).position, GameConfig.GroundLayerMask);
            float pinchDist = Vector3.Distance(touchPoint0, touchPoint1);

            if (!pinchStarted)
            {
                oldPinchDist = pinchDist;
                pinchStarted = true;
            }
            else
            {
                float delta = oldPinchDist - pinchDist;
                newZoom = mainCamera.orthographicSize + delta / 2;
            }
        }
    }

    private void CheckTouchRelesase()
    {
        int releaseTouchIndex = -1;
        if (Input.touchCount == 2)
        {
            for (int i = 0; i < 2; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Ended)
                {
                    releaseTouchIndex = i;
                }
            }
        }
        if (releaseTouchIndex > -1)
        {    
            dragStartPos = TryGetRayCastHitPoint(Input.GetTouch(releaseTouchIndex == 0 ? 1 : 0).position, GameConfig.GroundLayerMask);
            groundDragStarted = true;
        }
    }

    public void SetZoomAndSize(float _zoomMin, float _zoomMax, int _sizeMinX, int _sizeMaxX, int _sizeMinY, int _sizeMaxY)
    {
        minZoomFactor = _zoomMin;
        maxZoomFactor = _zoomMax;

        mapSizeMinX = _sizeMinX;
        mapSizeMaxX = _sizeMaxX;
        mapSizeMinY = _sizeMinY;
        mapSizeMaxY = _sizeMaxY;
    }

    public void SetTouchAction(System.Action _touchAction, System.Action _dragStartAction)
    {
        touchAction = _touchAction;
        dragStartAction = _dragStartAction;
    }

    public void SetFollowObject(GameObject _target, System.Action _targetAction)
    {
        followTarget = _target;
        followTargetAction = _targetAction;
        newPos = _target.transform.position;
    }

    public void CancelFollowTarget()
    {
        followTarget = null;
        followTargetAction = null;
        newPos = transform.position;
    }
    //private void ZoomCamera()
    //{
    //    Touch touchZero = Input.GetTouch(0);
    //    Touch touchOne = Input.GetTouch(1);

    //    var touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
    //    var touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

    //    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
    //    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

    //    float deltamagnitudeDiff = touchDeltaMag - prevTouchDeltaMag;

    //    newZoom += zoomAmount * deltamagnitudeDiff;
    //}

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

    public GameObject TryGetRayCastObject(Vector2 _touchPoint, int _layerMask)
    {
        var mousePos = mainCamera.ScreenToWorldPoint(_touchPoint);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, transform.forward, 15f, layerMask: _layerMask);
        if (hit && hit.collider != null)
        {
            return hit.collider.gameObject;
        }
        return null;
    }
}
