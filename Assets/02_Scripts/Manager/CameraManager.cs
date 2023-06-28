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

    private static Vector3 PositiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    private CancellationTokenSource cts = new CancellationTokenSource();

    private void Start()
    {
        bool dragStarted = false;
        Vector3 dragStartPos = Vector3.zero;
        Vector3 newPos = Vector3.zero;
        Vector3 newZoom = Vector3.one;

        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.Update).ForEachAwaitAsync(async _ =>
        {
#if UNITY_EDITOR
            PanCamera();
#else
            if (Input.touchCount == 1)
            {
            PanCamera();
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
                        Debug.Log($"newPos 0 {newPos} dragStartPos {dragStartPos} hitPoint {hitPoint}");
                        newPos = transform.position + dragStartPos - hitPoint;
                        Debug.Log($"newPos 1 {newPos}");
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    dragStarted = false;
                }
            }

        }, cts.Token).Forget();
        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate).ForEachAwaitAsync(async _ =>
        {
            try
            {
                Debug.Log($"newPos 2 {newPos}");
                transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * dragSpeed);
            }
            catch
            {
                Debug.Log("re");
            }
            
        }, cts.Token).Forget();
    }


    //    private void Start()
    //    {
    //        Vector3 tabGroundStartPosition = Vector3.zero;
    //        int previousTouchCount = 0;
    //        Vector3 previoursPanPoint = Vector3.zero;
    //        Vector3 panVelocity = Vector3.zero;
    //        bool isPanningStarted = false;
    //        Vector3 newPos = Vector3.zero;

    //        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.Update).ForEachAwaitAsync(async _ =>
    //        {
    //#if !UNITY_EDITOR
    //            UpdatePan();
    //#else
    //            if (Input.touchCount == 1)
    //            {
    //                UpdatePan();
    //            }
    //            else if (Input.touchCount == 2)
    //            {
    //            }
    //#endif

    //        }, cts.Token).Forget();

    //        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.FixedUpdate).ForEachAwaitAsync(async _ =>
    //        {
    //        }, cts.Token).Forget();

    //        void UpdatePan()
    //        {
    //            int touchCount = Input.touchCount;
    //            bool isInEditor = false;
    //            bool touchCountChanged = false;
    //            bool canPan;
    //            Vector2 touchPosition;

    //            if (Input.touchCount == 0)
    //            {
    //                if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)))
    //                {
    //                    touchCount = 1;
    //                    isInEditor = true;
    //                }
    //                else
    //                {
    //                    touchCount = 0;
    //                }
    //            }
    //            else
    //            {
    //                if (Input.GetTouch(0).phase == TouchPhase.Ended)
    //                {
    //                    touchCount = 0;
    //                }
    //                else
    //                {
    //                    touchCount = Input.touchCount;
    //                }
    //            }

    //            if (touchCount != previousTouchCount)
    //            {
    //                if (touchCount > 0)
    //                {
    //                    touchCountChanged = true;
    //                }
    //            }

    //            if (isInEditor)
    //            {
    //                touchPosition = Input.mousePosition;
    //            }
    //            else
    //            {
    //                if (touchCount > 0)
    //                {
    //                    if (touchCount == 1)
    //                    {
    //                        touchPosition = Input.GetTouch(0).position;
    //                    }
    //                    else
    //                    {
    //                        touchPosition = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2.0f;
    //                    }
    //                }
    //                else
    //                {
    //                    touchPosition = Vector2.zero;
    //                }
    //            }

    //            canPan = touchCount > 0;
    //            previousTouchCount = touchCount;

    //            if (canPan)
    //            {
    //                Vector3 hitPoint = TryGetRayCastHit(touchPosition, GameConfig.GroundLayerMask);

    //                if (hitPoint == Vector3.positiveInfinity)
    //                    return;

    //                if (touchCountChanged)
    //                {
    //                    tabGroundStartPosition = hitPoint;
    //                    previoursPanPoint = hitPoint;
    //                }

    //                if (!isPanningStarted && (tabGroundStartPosition - hitPoint).magnitude > 2f)
    //                {
    //                    isPanningStarted = true;
    //                    previoursPanPoint = hitPoint;
    //                }

    //                if (isPanningStarted)
    //                {
    //                    OnScenePan(hitPoint);
    //                }
    //            }
    //            else
    //            {
    //                if (isPanningStarted)
    //                {
    //                    isPanningStarted = false;
    //                    OnScenePanEnded();
    //                }
    //                UpdatePanInertia();
    //            }
    //        }

    //        void OnScenePanEnded()
    //        {
    //            Debug.Log($"stop _panVelocity{panVelocity}");
    //        }

    //        void OnScenePan(Vector3 newPoint)
    //        {
    //            Vector3 delta = previoursPanPoint - newPoint;
    //            Debug.Log($"_previous {previoursPanPoint} evtpoint {newPoint} delta {delta}");
    //            mainCamera.transform.localPosition += delta;
    //            panVelocity = delta;
    //            ClampCamera();
    //        }

    //        void UpdatePanInertia()
    //        {
    //            if (panVelocity.magnitude < panFactor)
    //            {
    //                panVelocity = Vector3.zero;
    //            }

    //            if (panVelocity != Vector3.zero)
    //            {
    //                panVelocity = Vector3.Lerp(panVelocity, Vector3.zero, Time.deltaTime * panReduceFactor);
    //                mainCamera.transform.localPosition += panVelocity;
    //                ClampCamera();
    //            }
    //        }
    //    }

    private void OnDestroy()
    {
        cts.Clear();
    }

    private void ClampCamera()
    {
        return;
        float worldSizePerPixel = 2 * mainCamera.orthographicSize / (float)Screen.height;
        var leftTopClampScreenPos = mainCamera.WorldToScreenPoint(CameraBoundary.Instance.CameraClampTopLeftPosition);
        if (leftTopClampScreenPos.x > 0)
        {
            return;
            float deltaFactor = leftTopClampScreenPos.x * worldSizePerPixel;
            var delta = new Vector3(deltaFactor, 0, 0);
            delta = mainCamera.transform.TransformVector(delta);
            mainCamera.transform.localPosition += delta;
        }

        if (leftTopClampScreenPos.y < Screen.height)
        {
            return;
            float deltaFactor = (Screen.height - leftTopClampScreenPos.y) * worldSizePerPixel;
            var delta = new Vector3(0, deltaFactor, 0);
            delta = mainCamera.transform.TransformVector(delta);
            mainCamera.transform.localPosition -= delta;
        }

        var rightBottomClampScreenPos = mainCamera.WorldToScreenPoint(CameraBoundary.Instance.CameraClampBottomRightBotPosition);
        if (rightBottomClampScreenPos.x < Screen.width)
        {
            return;
            float deltaFactor = (Screen.width - rightBottomClampScreenPos.x) * worldSizePerPixel;
            var delta = new Vector3(deltaFactor, 0, 0);
            delta = mainCamera.transform.TransformVector(delta);
            mainCamera.transform.localPosition -= delta;
        }

        if (rightBottomClampScreenPos.y > 0)
        {
            return;
            float deltaFactor = rightBottomClampScreenPos.y * worldSizePerPixel;
            var delta = new Vector3(0, deltaFactor, 0);
            delta = mainCamera.transform.TransformVector(delta);
            mainCamera.transform.localPosition += delta;
        }
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
}
