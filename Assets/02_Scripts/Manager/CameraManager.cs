using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System.Threading;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float panFactor = 0.05f;
    [SerializeField] private float panReduceFactor = 10f;

    [SerializeField] private int zoomSpeed = 1;
    [SerializeField] private int dragSpeed = 5;

    [SerializeField] private int mapSizeMinX = -20;
    [SerializeField] private int mapSizeMinZ = -20;
    [SerializeField] private int mapSizeMaxX = 20;
    [SerializeField] private int mapSizeMaxZ = 20;

    private readonly float maxZoomFactor = 50;
    private readonly float minZoomFactor = 3;
    private readonly float clampZoomOffset = 2.0f;
    private float oldZoom;
    private bool pinchStarted;
    private float oldPinchDist;

    private static Vector3 PositiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    private CancellationTokenSource cts = new CancellationTokenSource();

    private void Start()
    {
        bool dragStarted = false;
        Vector3 dragStartPos = Vector3.zero;
        Vector3 newPos = Vector3.zero;
        Vector3 newZoom = Vector3.one;

        int cnt = 0;
        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.Update).ForEachAwaitAsync(async _ =>
        {
#if UNITY_EDITOR
            PanCamera();
            UpdateZoom();
#else
            if (Input.touchCount == 1)
            {
                PanCamera();
                UpdateZoom();
            }
            else if (Input.touchCount == 2)
            {
            }
#endif

            void PanCamera()
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 hitPoint = TryGetRayCastHit(Input.mousePosition, GameConfig.GroundLayerMask);
                    if (!hitPoint.Equals(PositiveInfinityVector))
                    {
                        dragStartPos = hitPoint;
                        dragStarted = true;
                    }
                }

                if (dragStarted && Input.GetMouseButton(0))
                {
                    Vector3 hitPoint = TryGetRayCastHit(Input.mousePosition, GameConfig.GroundLayerMask);
                    if (!hitPoint.Equals(PositiveInfinityVector))
                    {
                        newPos = transform.position + dragStartPos - hitPoint;
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    dragStarted = false;
                }
            }

            void UpdateZoom()
            {
                float newZoom = mainCamera.orthographicSize;
                //Editor
                float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
                if (scrollAmount != 0)
                {
                    newZoom = newZoom - scrollAmount;
                }

                if (Input.touchCount == 0 || Input.touchCount == 1)
                {
                    pinchStarted = false;
                }

                if (Input.touchCount == 2)
                {
                    var touchPoint0 = TryGetRayCastHit(Input.GetTouch(0).position, GameConfig.GroundLayerMask);
                    var touchPoint1 = TryGetRayCastHit(Input.GetTouch(1).position, GameConfig.GroundLayerMask);
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

                newZoom = Mathf.Clamp(newZoom, minZoomFactor, maxZoomFactor);

                if (newZoom < minZoomFactor + clampZoomOffset)
                {
                    newZoom = Mathf.Lerp(newZoom, this.minZoomFactor + clampZoomOffset, Time.deltaTime * 2);

                }
                else if (newZoom > maxZoomFactor - clampZoomOffset)
                {
                    newZoom = Mathf.Lerp(newZoom, this.maxZoomFactor - clampZoomOffset, Time.deltaTime * 2);
                }

                if (oldZoom != newZoom)
                {
                    mainCamera.orthographicSize = newZoom;
                    oldZoom = newZoom;
                }
            }

        }, cts.Token).Forget();
        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate).ForEachAwaitAsync(async _ =>
        {
            newPos = new Vector3(Mathf.Clamp(newPos.x, mapSizeMinX, mapSizeMaxX), Mathf.Clamp(newPos.y, 0, 0), Mathf.Clamp(newPos.z, mapSizeMinZ, mapSizeMaxZ));
            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * dragSpeed);
        }, cts.Token).Forget();
    }

    private void OnDestroy()
    {
        cts.Clear();
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
            return PositiveInfinityVector;
        }            
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
