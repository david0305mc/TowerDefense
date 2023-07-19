using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System.Threading;
using UnityEngine.EventSystems;

using Game;
public class CameraManager : SingletonMono<CameraManager>
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject TEST_SRC_OBJ;
    [SerializeField] private GameObject TEST_DST_OBJ;
    public Camera MainCamera { get { return mainCamera; } }

    [SerializeField] private float zoomSpeed = 0.02f;
    [SerializeField] private int dragSpeed = 20;

    [SerializeField] private int mapSizeMinX = -20;
    [SerializeField] private int mapSizeMinZ = -20;
    [SerializeField] private int mapSizeMaxX = 20;
    [SerializeField] private int mapSizeMaxZ = 20;

    private readonly float maxZoomFactor = 50;
    private readonly float minZoomFactor = 3;

    private float minimumMoveDistanceForItemMove = 0.2f;

    private static Vector3 PositiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    private CancellationTokenSource cts = new CancellationTokenSource();

    //private void Update()
    //{
    //    Vector2 a = GameUtil.GetScreenPosition(CameraManager.Instance.MainCamera, TEST_SRC_OBJ.transform.position - new Vector3(0, 0, 1));
    //    Vector2 b = GameUtil.GetScreenPosition(CameraManager.Instance.MainCamera, TEST_SRC_OBJ.transform.position);
    //    Vector2 c = GameUtil.GetScreenPosition(CameraManager.Instance.MainCamera, TEST_DST_OBJ.transform.position);

    //    float angle = GameUtil.ClockwiseAngleOf3Points(a, b, c);
    //    Debug.Log($"angle {angle}");
    //}
    private void Start()
    {
        bool groundDragStarted = false;
        bool itemDragStarted = false;    
        bool pinchStarted = false;
        float oldPinchDist = 0f;
        Vector3 dragStartPos = Vector3.zero;
        Vector3 newPos = Vector3.zero;
        float newZoom = 10f;
        Vector3 oldPos = newPos;
        float oldZoom = newZoom;

        BaseObj selectedObj = default;

        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.Update).ForEachAwaitAsync(async _ =>
        {
            if (IsUsingUI())
            {
                return;
            }
            
            UpdateOneTouch();

#if UNITY_EDITOR
            InputEditor();
#else
            UpdateTwoTouch();
#endif
            void UpdateUITouch()
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    var hit = Physics2D.Raycast(mousePos, Vector3.zero, 100, GameConfig.UILayerMask);
                    if (hit.collider != null)
                    {
                        ShopItemCell itemCell = hit.collider.GetComponent<ShopItemCell>();
                        if (itemCell != null)
                        {
                            GameManager.Instance.SpawnCharacter();
                        }
                    }
                }

            }

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

                    Vector3 groudHitPoint = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
                    if (!groudHitPoint.Equals(PositiveInfinityVector))
                    {
                        dragStartPos = groudHitPoint;
                        if (selectedObj == default)
                        {
                            groundDragStarted = true;
                        }
                    }
                }

                if (Input.GetMouseButton(0))
                {
                    if (selectedObj != default)
                    {
                        var groundPos = TryGetRayCastHitPoint(Input.mousePosition, GameConfig.GroundLayerMask);
                        
                        if (!itemDragStarted)
                        {
                            if (Vector3.Distance(groundPos, dragStartPos) > minimumMoveDistanceForItemMove)
                            {
                                itemDragStarted = true;
                            }
                        }
                        else 
                        {
                            selectedObj.transform.SetPosition(groundPos);
                        }
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
                    itemDragStarted = false;
                    selectedObj = default;
                    dragStartPos = PositiveInfinityVector;
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
                        newZoom = mainCamera.orthographicSize + delta / 2;
                    }
                }
            }

            void InputEditor()
            {
                float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * 10;
                if (scrollAmount != 0)
                {
                    newZoom = newZoom - scrollAmount;
                }
            }
        }, cts.Token).Forget();
        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate).ForEachAwaitAsync(async _ =>
        {
            newPos = new Vector3(Mathf.Clamp(newPos.x, mapSizeMinX, mapSizeMaxX), Mathf.Clamp(newPos.y, 0, 0), Mathf.Clamp(newPos.z, mapSizeMinZ, mapSizeMaxZ));
            newZoom = Mathf.Clamp(newZoom, minZoomFactor, maxZoomFactor);
            if (!newPos.Equals(oldPos))
            {
                transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * dragSpeed);
            }

            if (!newZoom.Equals(oldZoom))
            {
                mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, newZoom, Time.deltaTime * zoomSpeed);
            }
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

}
