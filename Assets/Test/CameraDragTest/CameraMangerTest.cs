using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
    

public class CameraMangerTest : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI minText;
    [SerializeField] private TextMeshProUGUI maxText;
    private Vector3 PositiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

    public class CameraEvent
    {
        public Vector3 point;
        public GameObject baseItem;
    }

    [SerializeField] private EventSystem eventSystem; 

    private Camera mainCamera;
    public Camera MainCamera { get { return mainCamera; } }


    public UnityAction<CameraEvent> OnItemTap;
    public UnityAction<CameraEvent> OnItemDragStart;
    public UnityAction<CameraEvent> OnItemDrag;
    public UnityAction<CameraEvent> OnItemDragStop;
    public UnityAction<CameraEvent> OnTapGround;

    private float screenRatio = Screen.width / Screen.height;
    private Vector2 _defaultTouchPos = new Vector2(9999, 9999);
    private float _minimumMoveDistanceForItemMove = 0.2f;
    private float _maxZoomFactor = 10;
    private float _minZoomFactor = 5;
    private float _clampZoomOffset = 2.0f;

    private Vector3 _tapItemStartPos;
    private Vector3 _tapGroundStartPosition;

    private bool _isTappedBaseItem;
    private bool _isDraggingBaseItem;
    private bool _isPanningScene;



    private GameObject _selectedBaseItem;


    private Vector3 _previousPanPoint;
    private Vector3 _panVelocity = Vector3.zero;


    // Touch Count
    private int _previousTouchCount = 0;
    private bool _touchCountChanged;
    private Vector2 _touchPosition;
    private bool _canPan;


    // Scene Pan
    private bool _isPanningSceneStarted;

    // Zoom
    private Vector3 _touchPoint1;
    private Vector3 _touchPoint2;
    private bool _isZoomingStarted;
    private float _previousPinchDistance;
    private float _oldZoom = -1;

    // UpdateBaseItem Move
    private GameObject _tapStartRaycastedItem = null;
    private bool _isDragItemStarted;
    private bool _baseItemMoved;
    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        Application.targetFrameRate = 120;
    }

    void Update()
    {
        if (this.IsUsingUI())
        {
            return;
        }

        this.UpdateBaseItemTap();
        this.UpdateBaseItemMove();
        this.UpdateGroundTap();
        this.UpdateScenePan();
        this.UpdateSceneZoom();

        Debug.Log($"Main Camera {mainCamera.transform.position}");
    }

    public bool IsUsingUI()
    {
        if (this._isDraggingBaseItem)
        {
            return false;
        }

        if (_isPanningSceneStarted)
        {
            return false;
        }

        return (eventSystem.IsPointerOverGameObject() || eventSystem.IsPointerOverGameObject(0));
    }
    private void _RefreshTouchValues()
    {
        this._touchCountChanged = false;
        int touchCount = 0;
        bool isInEditor = false;


        if (Input.touchCount == 0)
        {
            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)))
            {
                //editor
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

        if (touchCount != this._previousTouchCount)
        {
            if (touchCount != 0)
            {
                this._touchCountChanged = true;
            }
        }

        if (isInEditor)
        {
            this._touchPosition = (Vector2)Input.mousePosition;
        }
        else
        {
            if (touchCount == 1)
            {
                this._touchPosition = Input.GetTouch(0).position;
            }
            else if (touchCount >= 2)
            {
                this._touchPosition = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2.0f;
            }
        }

        this._canPan = (touchCount > 0);

        this._previousTouchCount = touchCount;
    }

    public void UpdateBaseItemMove()
    {

        if (Input.GetMouseButtonDown(0))
        {
            this._tapItemStartPos = this.TryGetRayCastHitPoint(Input.mousePosition, Game.GameConfig.GroundLayerMask);
            this._tapStartRaycastedItem = this.TryGetRaycastHitBaseItem(Input.mousePosition, Game.GameConfig.ItemLayerMask);
            this._isDraggingBaseItem = false;
            this._isDragItemStarted = false;
        }


        if (Input.GetMouseButton(0) && this._tapItemStartPos != PositiveInfinityVector)
        {
            if (this._isTappedBaseItem && this._selectedBaseItem == this._tapStartRaycastedItem)
            {
                Vector3 currentTapPosition = this.TryGetRayCastHitPoint(Input.mousePosition, Game.GameConfig.GroundLayerMask);
                if (Vector3.Distance(this._tapItemStartPos, currentTapPosition) >= _minimumMoveDistanceForItemMove)
                {

                    CameraEvent evt = new CameraEvent()
                    {
                        point = currentTapPosition,
                        baseItem = this._selectedBaseItem
                    };

                    if (!this._isDragItemStarted)
                    {
                        //						Debug.Log ("BaseItemDragStart");
                        this._isDragItemStarted = true;
                        if (this.OnItemDragStart != null)
                        {
                            this.OnItemDragStart.Invoke(evt);
                        }
                    }

                    //					Debug.Log ("BaseItemDrag");
                    this._isDraggingBaseItem = true;
                    if (this.OnItemDrag != null)
                    {
                        this.OnItemDrag.Invoke(evt);
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            this._tapItemStartPos = PositiveInfinityVector;
            if (_isDragItemStarted)
            {
                //				Debug.Log ("BaseItemDragStop");
                this._isDragItemStarted = false;
                this._isDraggingBaseItem = false;
                if (this.OnItemDragStop != null)
                {
                    this.OnItemDragStop.Invoke(null);
                }
            }
        }
    }
    public void UpdateBaseItemTap()
    {
        if (!Input.GetMouseButtonUp(0))
        {
            return;
        }


        if (this._isPanningSceneStarted)
        {
            return;
        }

        if (this._isDraggingBaseItem)
        {
            return;
        }

        if (this.IsUsingUI())
        {
            return;
        }

        GameObject baseItemTapped = this.TryGetRaycastHitBaseItem(Input.mousePosition, Game.GameConfig.ItemLayerMask);
        if (baseItemTapped != null)
        {
            this._isTappedBaseItem = true;

            this._selectedBaseItem = baseItemTapped;

            CameraEvent evt = new CameraEvent()
            {
                baseItem = baseItemTapped
            };
            if (this.OnItemTap != null)
            {
                this.OnItemTap.Invoke(evt);
            }


        }
        else
        {
            this._isTappedBaseItem = false;
            this._selectedBaseItem = null;
        }
    }

    public void UpdateSceneZoom()
    {

        if (this._isDraggingBaseItem)
        {
            return;
        }

        float newZoom = this.MainCamera.orthographicSize;

        //Editor
        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        if (scrollAmount != 0)
        {
            newZoom = newZoom - scrollAmount;
        }

        //Android
        if (Input.touchCount == 0)
        {
            this._isZoomingStarted = false;
        }

        if (Input.touchCount == 2)
        {
            _touchPoint1 = TryGetRayCastHitPoint(Input.GetTouch(0).position, Game.GameConfig.GroundLayerMask);
            _touchPoint2 = TryGetRayCastHitPoint(Input.GetTouch(1).position, Game.GameConfig.GroundLayerMask);
            if (!_isZoomingStarted)
            {
                this._isZoomingStarted = true;
                this._previousPinchDistance = (_touchPoint2 - _touchPoint1).magnitude;
            }
        }

        //if (screenRatio <= 0)
        //{
        //    Debug.LogError("screenRatio <= 0");
        //}
        if (this._isZoomingStarted)
        {
            float _currentPinchDistance = (_touchPoint2 - _touchPoint1).magnitude;
            float delta = this._previousPinchDistance - _currentPinchDistance;
            
            newZoom = this.MainCamera.orthographicSize + (delta / 2);
        }

        //clamp zoom
        newZoom = Mathf.Clamp(newZoom - scrollAmount, this._minZoomFactor, this._maxZoomFactor);
        if (newZoom < this._minZoomFactor + _clampZoomOffset)
        {
            newZoom = Mathf.Lerp(newZoom, this._minZoomFactor + _clampZoomOffset, Time.deltaTime * 2);

        }
        else if (newZoom > this._maxZoomFactor - _clampZoomOffset)
        {
            newZoom = Mathf.Lerp(newZoom, this._maxZoomFactor - _clampZoomOffset, Time.deltaTime * 2);
        }

        if (this._oldZoom != newZoom)
        {
            Debug.Log($"newZoom {newZoom}");
            this.MainCamera.orthographicSize = newZoom;
            this.ClampCamera();
            this._oldZoom = newZoom;
        }
    }

    private GameObject TryGetRaycastHitBaseItem(Vector2 touch, int _layerMask)
    {
        var mousePos = mainCamera.ScreenToWorldPoint(touch);

        RaycastHit2D hit = Physics2D.Raycast(mousePos, transform.forward, 15f, layerMask: _layerMask);
        if (hit)
        {
            return hit.collider.gameObject;
        }

        return null;
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
    private Vector3 TryGetRaycastHitBaseGround(Vector2 touch)
    {
        RaycastHit hit;
        var ray = mainCamera.ScreenPointToRay(touch);

        if (Physics.Raycast(ray, out hit, 1000, Game.GameConfig.GroundLayerMask))
        {
            return hit.point;
        }
        else
        {
            return PositiveInfinityVector;
        }
    }


    public void UpdateGroundTap()
    {
        if (this._isTappedBaseItem)
        {
            return;
        }

        if (this._isDraggingBaseItem)
        {
            return;
        }

        if (this._isPanningScene)
        {
            return;
        }

        if (this._isPanningSceneStarted)
        {
            return;
        }

        if (!Input.GetMouseButtonUp(0))
        {
            return;
        }

        Vector3 tapPosition = this.TryGetRayCastHitPoint(Input.mousePosition, Game.GameConfig.GroundLayerMask);
        if (tapPosition != PositiveInfinityVector)
        {
            //			Debug.Log ("GroundTap");
            CameraEvent evt = new CameraEvent()
            {
                point = tapPosition
            };
            if (this.OnTapGround != null)
            {
                this.OnTapGround.Invoke(evt);
            }
        }
    }

    public void UpdateScenePan()
    {
        this._RefreshTouchValues();

        if (this._isDraggingBaseItem)
        {
            return;
        }

        if (this._touchCountChanged)
        {
            this._tapGroundStartPosition = this.TryGetRayCastHitPoint(this._touchPosition, Game.GameConfig.GroundLayerMask);
        }

        if (this._canPan)
        {
            Vector3 currentTapPosition = this.TryGetRayCastHitPoint(this._touchPosition, Game.GameConfig.GroundLayerMask);

            if (this._touchCountChanged)
            {
                CameraEvent evt = new CameraEvent()
                {
                    point = currentTapPosition
                };
                this.OnChangeTouchCountScenePan(evt);
            }

            if (!this._isPanningSceneStarted && Vector3.Distance(this._tapGroundStartPosition, currentTapPosition) >= 0.5f)
            {
                this._isPanningSceneStarted = true;
                this._previousPanPoint = currentTapPosition;
            }

            if (this._isPanningSceneStarted)
            {
                CameraEvent evt = new CameraEvent()
                {
                    point = currentTapPosition
                };

                this._isPanningScene = true;
                this.OnScenePan(evt);
            }

        }
        else
        {
            this._isPanningScene = false;

            if (this._isPanningSceneStarted)
            {
                this._isPanningSceneStarted = false;
                this.OnStopScenePan(null);
            }
        }

        if (!this._isPanningScene)
        {
            this.UpdatePanInertia();
        }
    }
    public void OnChangeTouchCountScenePan(CameraEvent evt)
    {
        this._previousPanPoint = evt.point;
    }

    public void OnStopScenePan(CameraEvent evt)
    {
        //		Debug.Log ("OnStopPan");
    }

    public void UpdatePanInertia()
    {
        if (this._panVelocity.magnitude < 0.01f)
        {
            this._panVelocity = Vector3.zero;
        }
        if (this._panVelocity != Vector3.zero)
        {
            this._panVelocity = Vector3.Lerp(_panVelocity, Vector3.zero, Time.deltaTime * 10);
            this.MainCamera.transform.localPosition += this._panVelocity;
            this.ClampCamera();
        }
    }
    public void OnScenePan(CameraEvent evt)
    {

        Vector3 delta = this._previousPanPoint - evt.point;
        //delta = delta * 0.1f;
        Debug.Log($"OnScenePan this.delta {delta.x} {delta.y}");
        this.MainCamera.transform.localPosition += delta;
        this._panVelocity = delta;
        //		if(this._panVelocity.magnitude > 0.5f){
        //			this._panVelocity = this._panVelocity.normalized * 0.5f;
        //		}
        this.ClampCamera();
    }

    public void ClampCamera()
    {
        //		return;
        //float worldSizePerPixel = 2 * this.MainCamera.orthographicSize / (float)Screen.height;

        ////clamp camera left and top
        //Vector3 leftClampScreenPos = this.MainCamera.WorldToScreenPoint(CameraBoundScript.instance.CameraClampTopLeftPosition);
        //if (leftClampScreenPos.x > 0)
        //{
        //    float deltaFactor = leftClampScreenPos.x * worldSizePerPixel;
        //    Vector3 delta = new Vector3(deltaFactor, 0, 0);
        //    delta = this.MainCamera.transform.TransformVector(delta);
        //    this.MainCamera.transform.localPosition += delta;
        //}

        //if (leftClampScreenPos.y < Screen.height)
        //{
        //    float deltaFactor = (Screen.height - leftClampScreenPos.y) * worldSizePerPixel;
        //    Vector3 delta = new Vector3(-deltaFactor, 0, -deltaFactor);
        //    this.MainCamera.transform.localPosition += delta;
        //}
        ////clamp camera right and bottom
        //Vector3 rightClampScreenPos = this.MainCamera.WorldToScreenPoint(CameraBoundScript.instance.CameraClampBottomRightPosition);

        //if (rightClampScreenPos.x < Screen.width)
        //{
        //    float deltaFactor = (rightClampScreenPos.x - Screen.width) * worldSizePerPixel;
        //    Vector3 delta = new Vector3(deltaFactor, 0, 0);
        //    delta = this.MainCamera.transform.TransformVector(delta);
        //    this.MainCamera.transform.localPosition += delta;
        //}

        //if (rightClampScreenPos.y > 0)
        //{
        //    float deltaFactor = rightClampScreenPos.y * worldSizePerPixel;
        //    Vector3 delta = new Vector3(deltaFactor, 0, deltaFactor);
        //    this.MainCamera.transform.localPosition += delta;
        //}
    }

    public void OnClickBtnTest()
    {
        this.MainCamera.orthographicSize = 5f;
        
    }
    public void OnClickBtnTest2()
    {
        _minZoomFactor = int.Parse(minText.text);
        _maxZoomFactor = int.Parse(maxText.text);

    }
}
