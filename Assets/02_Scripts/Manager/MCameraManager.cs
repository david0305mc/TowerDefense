using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.EventSystems;
using Game;
using UniRx;

public class MCameraManager : SingletonMono<MCameraManager>
{
    [SerializeField] private int mapSizeMinX = -20;
    [SerializeField] private int mapSizeMaxX = 20;
    [SerializeField] private int mapSizeMinY = -10;
    [SerializeField] private int mapSizeMaxY = 30;

    [SerializeField] private float followSpeed = 7f;
    [SerializeField] private float panInertiafactor = 5f;

    [SerializeField] private float maxZoomFactor = 50;
    [SerializeField] private float minZoomFactor = 3;
    [SerializeField] private float zoomSpeed = 10f;


    private static Vector3 PositiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    private Camera mainCamera;

    private Vector3 newPos;
    private Vector3 oldPos;
    private Vector3 orgPos;

    private float newZoom;
    private float oldZoom;

    bool pinchStarted = false;
    float oldPinchDist = 0f;

    private Vector3 dragStartPos = Vector3.zero;
    private Vector3 dragStartInputPos = Vector3.zero;
    private bool groundDragStarted = false;

    private Vector3 panVelocity = Vector3.zero;
    private Vector3 previousPanPoint = Vector3.zero;

    private System.Action touchAction;
    private System.Action dragStartAction;
    private GameObject followTarget;
    public GameObject FollowTarget => followTarget;
    private Vector3 followOffeset;
    private System.Action followTargetAction;
    private bool keepFollow;
    private float followSpeedFactor;
    private float uniVelocityElapse;
    private bool isUniVelocity;
    public float ZoomSize => newZoom;

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
        followSpeedFactor = 1f;
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

        if (TouchBlockManager.Instance.IsLock())
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

    private void LateUpdate()
    //private void FixedUpdate()
    {
        if (followTarget != null)
        {
            newPos = followTarget.transform.position + followOffeset;
        }

        if (!groundDragStarted)
        {
            if (panVelocity.magnitude < 0.01f)
            {
                panVelocity = Vector3.zero;
            }
            if (panVelocity != Vector3.zero)
            {
                panVelocity = Vector3.Lerp(panVelocity, Vector3.zero, Time.deltaTime * 20);
                newPos -= panVelocity;
            }
        }

        newPos = new Vector3(Mathf.Clamp(newPos.x, mapSizeMinX, mapSizeMaxX), Mathf.Clamp(newPos.y, mapSizeMinY, mapSizeMaxY), -10);
        if (!newPos.Equals(oldPos))
        {
            if (isUniVelocity)
            {
                var dist = Vector2.Distance(orgPos, newPos);
                uniVelocityElapse += (Time.unscaledDeltaTime / dist) * followSpeed;
                transform.position = Vector3.Lerp(orgPos, newPos, uniVelocityElapse);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * followSpeed * followSpeedFactor);
            }
            
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
            if (!keepFollow)
            {
                if (Vector2.Distance(transform.position, followTarget.transform.position + followOffeset) <= 0.1f)
                {
                    followSpeedFactor = 1f;
                    followTargetAction?.Invoke();
                    followTarget = null;
                    followTargetAction = null;
                }
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
        //else
        //{
        //    pinchStarted = false;
        //    float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * 10;
        //    if (scrollAmount != 0)
        //    {
        //        newZoom = newZoom - scrollAmount;
        //    }
        //}
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
                dragStartInputPos = Input.mousePosition;
                groundDragStarted = true;
                previousPanPoint = groudHitPoint;
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
                    panVelocity = (previousPanPoint - newPos) * panInertiafactor;
                    previousPanPoint = newPos;
                    if (!newPos.Equals(oldPos))
                    {
                        transform.position = newPos;
                        oldPos = transform.position;
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (Vector3.Distance(dragStartInputPos, Input.mousePosition) <= 10f)
            {
                touchAction?.Invoke();
            }
            groundDragStarted = false;
            dragStartPos = PositiveInfinityVector;
        }

        float scrollAmount = Input.mouseScrollDelta.y;
        if (scrollAmount != 0)
        {
            newZoom = newZoom - scrollAmount;
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

    public void SetPosition(Vector3 _pos)
    {
        newPos = _pos;
        oldPos = newPos;
    }

    public void SetZoomAndSize(float _zoomSize, float _zoomMin, float _zoomMax, int _sizeMinX, int _sizeMaxX, int _sizeMinY, int _sizeMaxY)
    { 
        minZoomFactor = _zoomMin;
        maxZoomFactor = _zoomMax;

        mapSizeMinX = _sizeMinX;
        mapSizeMaxX = _sizeMaxX;
        mapSizeMinY = _sizeMinY;
        mapSizeMaxY = _sizeMaxY;

        newPos = new Vector3(Mathf.Clamp(newPos.x, mapSizeMinX, mapSizeMaxX), Mathf.Clamp(newPos.y, mapSizeMinY, mapSizeMaxY), -10);
        if (!newPos.Equals(oldPos))
        {
            transform.position = newPos;
            oldPos = transform.position;
        }

        newZoom = Mathf.Clamp(_zoomSize, minZoomFactor, maxZoomFactor);
        if (!newZoom.Equals(oldZoom))
        {
            mainCamera.orthographicSize = newZoom;
            oldZoom = mainCamera.orthographicSize;
        }
    }

    public void SetTouchAction(System.Action _touchAction, System.Action _dragStartAction)
    {
        touchAction = _touchAction;
        dragStartAction = _dragStartAction;
    }

    public void SetFollowObject(GameObject _target, float _dragSpeedFactor,  bool _keepFollow, Vector3 _offset, System.Action _targetAction, bool _isUniVelocity = false)
    {
        followOffeset = _offset;
        keepFollow = _keepFollow;
        followTarget = _target;
        followTargetAction = _targetAction;
        orgPos = transform.position;
        newPos = followTarget.transform.position + _offset;
        followSpeedFactor = _dragSpeedFactor;
        isUniVelocity = _isUniVelocity;
        uniVelocityElapse = 0f;
    }

    public void CancelFollowTarget()
    {
        followTarget = null;
        followTargetAction = null;
        newPos = transform.position;
        followSpeedFactor = 1f;
        isUniVelocity = false;

        MessageDispather.Publish(EMessage.DeSelectUnitTarget);
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
        //return EventSystem.current.IsPointerOverGameObject();
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        if (results.Count > 0)
        {
            int count = 0;
            foreach (var item in results)
            {
                if (item.gameObject.layer == LayerMask.NameToLayer("UI"))
                {
                    count++;
                }
                if (item.gameObject.layer == LayerMask.NameToLayer("HUD"))
                {
                    continue;
                }
            }
            return count > 0;
        }
        else
        {
            return false;
        }
    }

    public Vector3 TryGetRayCastHitPoint(Vector2 _touchPoint, int _layerMask)
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

    public (Vector3, Vector3, Vector3, Vector3) GetBoundary()
    {
        Vector3 topLeft = new Vector3(mapSizeMinX, mapSizeMaxY);
        Vector3 topRight = new Vector3(mapSizeMaxX, mapSizeMaxY);
        Vector3 botRight = new Vector3(mapSizeMaxX, mapSizeMinY);
        Vector3 botLeft = new Vector3(mapSizeMinX, mapSizeMinY);
        return (topLeft, topRight, botRight, botLeft);
    }
}
