using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System.Threading;
using UnityEngine.EventSystems;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float panFactor = 0.05f;
    [SerializeField] private float panReduceFactor = 10f;

    [SerializeField] private float zoomSpeed = 0.02f;
    [SerializeField] private int dragSpeed = 20;

    [SerializeField] private int mapSizeMinX = -20;
    [SerializeField] private int mapSizeMinZ = -20;
    [SerializeField] private int mapSizeMaxX = 20;
    [SerializeField] private int mapSizeMaxZ = 20;

    private readonly float maxZoomFactor = 50;
    private readonly float minZoomFactor = 3;
    private readonly float clampZoomOffset = 2.0f;

    private float minimumMoveDistanceForItemMove = 0.2f;
    private float oldZoom;
    private bool pinchStarted;
    private float oldPinchDist;

    private static Vector3 PositiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    private CancellationTokenSource cts = new CancellationTokenSource();

    private void Start()
    {
        bool groundDragStarted = false;
        bool itemDragStarted = false;
        Vector3 dragStartPos = Vector3.zero;
        Vector3 newPos = Vector3.zero;
        //Vector3 newZoom = Vector3.one;
        float newZoomf = 10f;

        BaseObj selectedObj = default;
        Vector3 tapItemStartPos = default;
        bool isObjDragStarted = false;

        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.Update).ForEachAwaitAsync(async _ =>
        {
            UpdateOneTouch();

#if UNITY_EDITOR
            InputKeyboard();
#else
            UpdateTwoTouch();
#endif

            

//#if UNITY_EDITOR
//            PanCamera();
//            UpdateZoom();
//            UpdateBaseItemMove();
//            UpdateBaseItemTap();
//#else
//            if (Input.touchCount == 1)
//            {
//                PanCamera();
//                UpdateZoom();
//            }
//            else if (Input.touchCount == 2)
//            {
//            }
//#endif

            void UpdateOneTouch()
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var hitObj = TryGetRayCastHitObj(Input.mousePosition, GameConfig.ItemLayerMask);
                    if (hitObj != default)
                    {
                        BaseObj[] baseObj = hitObj.GetComponentsInParent<BaseObj>();
                        selectedObj = baseObj[0];
                    }
                    else
                    {
                        Vector3 groudHitPoint = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
                        if (!groudHitPoint.Equals(PositiveInfinityVector))
                        {
                            dragStartPos = groudHitPoint;
                            groundDragStarted = true;
                        }
                    }
                }

                if (Input.GetMouseButton(0))
                {
                    if (selectedObj != default)
                    {
                        var groundPos = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
                        selectedObj.transform.SetPosition(groundPos);
                    }
                    else
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
                }

                if (Input.GetMouseButtonUp(0))
                {
                    groundDragStarted = false;
                    selectedObj = default;
                }
            }

            void UpdateTwoTouch()
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
                        newZoomf = mainCamera.orthographicSize + delta / 2;
                    }
                }
            }

            void InputKeyboard()
            {
                float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * 10;
                if (scrollAmount != 0)
                {
                    newZoomf = newZoomf - scrollAmount;
                }
                newZoomf = Mathf.Clamp(newZoomf, minZoomFactor, maxZoomFactor);
            }
            void UpdateTouch()
            {

                if (!Input.GetMouseButtonUp(0))
                    return;

                if (Input.touchCount == 1)
                {
                    UpdateOneTouch();
                }
                else if (Input.touchCount == 2)
                {

                }
                
                // 원 터치

                // 터치다운
                // 아이템 선택
                // 그라운드 선택

                // 터치중
                // 최초 선택이 아이템
                // 최소 이동 거리를 넘었을 경우 
                // 아이템 드래그 시작

                // 최초 선택이 그라운드
                // Pan Camera

                // 터치 업
                // 아이템 드래그일경우
                // 타겟이 아이템인경우 롤백

                // 드래그 플래그 초기화

                // 투터치
                // 터치중
                // 카메라 줌
            }

            void TouchLogic()
            { 

                // UI인경우 리턴

                // 원 터치
                  
                    // 터치다운
                        // 아이템 선택
                        // 그라운드 선택

                    // 터치중
                        // 최초 선택이 아이템
                            // 최소 이동 거리를 넘었을 경우 
                                // 아이템 드래그 시작

                        // 최초 선택이 그라운드
                            // Pan Camera

                    // 터치 업
                        // 아이템 드래그일경우
                            // 타겟이 아이템인경우 롤백

                        // 드래그 플래그 초기화

                // 투터치
                    // 터치중
                        // 카메라 줌

            }

            void UpdateBaseItemTap()
            {
                if (!Input.GetMouseButtonUp(0))
                    return;

                if (groundDragStarted)
                    return;

                if (itemDragStarted)
                    return;

                if (this.IsUsingUI())
                {
                    return;
                }

                GameObject obj = TryGetRayCastHitObj(Input.mousePosition, GameConfig.ItemLayerMask);

                if (obj != default)
                {
                    BaseObj[] baseObj = obj.GetComponentsInParent<BaseObj>();
                    selectedObj = baseObj[0];

                    // objEvent
                }
                else
                {
                    selectedObj = null;
                }
            }

            void UpdateBaseItemMove()
            {

                if (Input.GetMouseButtonDown(0))
                {
                    tapItemStartPos = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
                    isObjDragStarted = false;
                }

                if (selectedObj == null)
                    return;

                if (Input.GetMouseButton(0) && !tapItemStartPos.Equals(PositiveInfinityVector))
                {
                    var tapItemCurrPos = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);

                    if (!isObjDragStarted)
                    {
                        if (Vector3.Distance(tapItemStartPos, tapItemCurrPos) >= minimumMoveDistanceForItemMove)
                        {
                            isObjDragStarted = true;
                            Debug.Log("DragStarted");
                        }
                    }

                    if (isObjDragStarted)
                    {
                        selectedObj.transform.position = tapItemCurrPos;
                        // Dragging
                        Debug.Log("Dragging");
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    tapItemStartPos = PositiveInfinityVector;
                    isObjDragStarted = false;
                }

            }

            void PanCamera()
            {
                if (selectedObj != null)
                    return;

                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 hitPoint = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
                    if (!hitPoint.Equals(PositiveInfinityVector))
                    {
                        dragStartPos = hitPoint;
                        groundDragStarted = true;
                    }
                }

                if (groundDragStarted && Input.GetMouseButton(0))
                {
                    Vector3 hitPoint = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
                    if (!hitPoint.Equals(PositiveInfinityVector))
                    {
                        newPos = transform.position + dragStartPos - hitPoint;
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    groundDragStarted = false;
                }
            }

            void UpdateZoom()
            {
                newZoomf = mainCamera.orthographicSize;
                //Editor
                float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * 5f;
                if (scrollAmount != 0)
                {
                    newZoomf = newZoomf - scrollAmount;
                }

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
                        newZoomf = mainCamera.orthographicSize + delta / 2;
                    }
                }

                newZoomf = Mathf.Clamp(newZoomf, minZoomFactor, maxZoomFactor);

                if (newZoomf < minZoomFactor + clampZoomOffset)
                {
                    newZoomf = Mathf.Lerp(newZoomf, this.minZoomFactor + clampZoomOffset, Time.deltaTime * 2);

                }
                else if (newZoomf > maxZoomFactor - clampZoomOffset)
                {
                    newZoomf = Mathf.Lerp(newZoomf, this.maxZoomFactor - clampZoomOffset, Time.deltaTime * 2);
                }

                if (oldZoom != newZoomf)
                {
                    mainCamera.orthographicSize = newZoomf;
                    oldZoom = newZoomf;
                }
            }

        }, cts.Token).Forget();
        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate).ForEachAwaitAsync(async _ =>
        {
            newPos = new Vector3(Mathf.Clamp(newPos.x, mapSizeMinX, mapSizeMaxX), Mathf.Clamp(newPos.y, 0, 0), Mathf.Clamp(newPos.z, mapSizeMinZ, mapSizeMaxZ));
            newZoomf = Mathf.Clamp(newZoomf, minZoomFactor, maxZoomFactor);
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, newZoomf, Time.deltaTime * zoomSpeed);
            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * dragSpeed);
        }, cts.Token).Forget();
    }

    private void OnDestroy()
    {
        cts.Clear();
    }

    private Vector3 TryGetRayCastHitPoint(Vector2 touchPoint, int layerMask)
    {
        RaycastHit hit;
        var ray = mainCamera.ScreenPointToRay(touchPoint);
        if (Physics.Raycast(ray, out hit, 1000, layerMask))
        {
            return hit.point;
        }
        else
        {
            return PositiveInfinityVector;
        }            
    }
    private GameObject TryGetRayCastHitObj(Vector2 touchPoint, int layerMask)
    {
        RaycastHit hit;
        var ray = mainCamera.ScreenPointToRay(touchPoint);
        if (Physics.Raycast(ray, out hit, 1000, layerMask))
        {
            return hit.collider.gameObject;
        }
        else
        {
            return default;
        }
    }

    public bool IsUsingUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }



    //void ClampCamera()
    //{

    //    Debug.Log($"transform.position {this.transform.position}");
    //    Debug.Log($"transform.localPosition {this.transform.localPosition}");
    //    //if (transform.position.x < -10)
    //    //{
    //    //    var delta = new Vector3(-10, 0, 0);
    //    //    newPos = transform.TransformVector(delta);
    //    //    //newPos.x = -10;
    //    //}
    //    return;
    //    float worldSizePerPixel = 2 * mainCamera.orthographicSize / (float)Screen.height;
    //    var leftTopClampScreenPos = mainCamera.WorldToScreenPoint(CameraBoundary.Instance.CameraClampTopLeftPosition);
    //    if (leftTopClampScreenPos.x > 0)
    //    {
    //        float deltaFactor = leftTopClampScreenPos.x * worldSizePerPixel;
    //        var delta = new Vector3(deltaFactor, 0, 0);

    //        Debug.Log($"delta {delta}");
    //        newPos += delta;
    //        //delta = transform.TransformVector(delta);
    //        //newPos += delta;
    //        //transform.position += delta;
    //    }
    //}
}
