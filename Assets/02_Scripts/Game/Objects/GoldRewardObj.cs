using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldRewardObj : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private Transform startTR;
    [SerializeField] private Transform secondTR;


    protected Rigidbody2D rigidBody2d;
    protected Vector3 srcPos;
    protected Vector3 dstPos;

    private float elapse;

    [Range(0.01f, 1f)]
    public float gizmoRadius = 0.3f;

    [Space, Range(2, 100)]
    public int samplePointCount = 100;

    private Vector3[] points;

    // Check Changed
    private int _prevLength = 0;
    private int _prevSampleCount = 0;
    private Vector3[] _prevPositions;

    // curve
    private Vector3[] curvePoints;
    private TrailRenderer trainRenderer;

    private Transform targetObj;
    protected Vector3 prevPos;
    private System.Action endAction;

    private float orgSpeed;
    private Vector3 orgScale;
    protected virtual void Awake()
    {
        rigidBody2d = GetComponent<Rigidbody2D>();
        trainRenderer = GetComponent<TrailRenderer>();
        orgSpeed = speed;
        orgScale = transform.localScale;
    }

    //public void SetData(GameObject _target)
    //{
    //    prevPos = _target.transform.position;
    //    elapse = 0f;
    //    srcPos = transform.position;
    //    dstPos = _target.transform.position;
    //}

    private void Update()
    {
        float cameraScale = MCameraManager.Instance.ZoomSize / Game.GameConfig.projectileRewardSizeFactor;
        speed = orgSpeed * cameraScale;
        transform.localScale = orgScale * cameraScale;
        trainRenderer.SetScale(transform.localScale.x);

        dstPos = targetObj.position;
        float dist = Vector2.Distance(srcPos, dstPos);
        elapse += Time.deltaTime / dist * speed;

        if (elapse >= 1f)
        {
            Lean.Pool.LeanPool.Despawn(gameObject);
            endAction?.Invoke();
            return;
        }

        UpdateMissile();
    }
    //private void Update()
    //{
    //    var pos = Vector2.Lerp(srcPos, dstPos, elapse);
    //    transform.SetPosition(pos);
    //}

    public void Shoot(Transform _targetObj, System.Action _endAction)
    {
        targetObj = _targetObj;
        endAction = _endAction;
        if (trainRenderer != null)
        {
            trainRenderer.Clear();
        }
        elapse = 0f;
        prevPos = targetObj.position;
        elapse = 0f;
        srcPos = transform.position;
        dstPos = targetObj.position;

        startTR.position = transform.position;
        startTR.rotation = GameUtil.LookAt2D(startTR.position, _targetObj.position, GameUtil.FacingDirection.DOWN);
        //startTR.SetPositionAndRotation(transform.position, );

        float cameraScale = MCameraManager.Instance.ZoomSize / Game.GameConfig.projectileRewardSizeFactor;
        secondTR.position = startTR.TransformPoint(new Vector2(Random.Range(0, 2) == 0 ? -5 * cameraScale : 5 * cameraScale, -5 * cameraScale));
        points = new Vector3[] { transform.position, secondTR.position, _targetObj.position };
        CalculateCurvePoints(samplePointCount);
        SavePrevious();
        UpdateMissile();
    }


    protected bool UpdateMissile()
    {
        //if (!base.UpdateMissile())
        //{
        //    return false;
        //}
        points[points.Length - 1] = dstPos;

        //if (PointsChanged())    
        CalculateCurvePoints(samplePointCount);

        float fLen = (curvePoints.Length - 1) * elapse;
        fLen = Mathf.Clamp((int)fLen, 0, curvePoints.Length - 1);
        var pos = curvePoints[(int)fLen];

        //rigidBody2d.MovePosition(pos);
        if (prevPos != pos)
        {
            if (rigidBody2d != null)
            {
                rigidBody2d.MovePosition(pos);
            }
            else
            {
                transform.SetPosition(pos);
            }
        }
        prevPos = pos;

        SavePrevious();
        return true;
    }



    #region Bezier Logic
    private bool PointsChanged()
    {
        if (points == null || _prevPositions == null || points.Length == 0) return false;

        if (_prevLength != points.Length)
            return true;

        if (_prevSampleCount != samplePointCount)
            return true;

        for (int i = 0; i < points.Length && i < _prevPositions.Length; i++)
        {
            if (Vector3.SqrMagnitude(points[i] - _prevPositions[i]) > 0.01f)
                return true;
        }
        return false;
    }
    private void CalculateCurvePoints(int count)
    {
        if (points == null || points.Length < 2) return;

        curvePoints = new Vector3[count + 1];
        float unit = 1.0f / count;

        ref Vector3[] P = ref points;

        int n = P.Length - 1;
        int[] C = GetCombinationValues(n); // nCi
        float[] T = new float[n + 1];      // t^i
        float[] U = new float[n + 1];      // (1-t)^i

        // Iterate curvePoints : 0 ~ count(200)
        int k = 0; float t = 0f;
        for (; k < count + 1; k++, t += unit)
        {
            curvePoints[k] = Vector3.zero;

            T[0] = 1f;
            U[0] = 1f;
            T[1] = t;
            U[1] = 1f - t;

            // T[i] = t^i
            // U[i] = (1 - t)^i
            for (int i = 2; i <= n; i++)
            {
                T[i] = T[i - 1] * T[1];
                U[i] = U[i - 1] * U[1];
            }

            // Iterate Bezier Points : 0 ~ n(number of points - 1)
            for (int i = 0; i <= n; i++)
            {
                curvePoints[k] += C[i] * T[i] * U[n - i] * P[i];
            }
        }
    }

    private int[] GetCombinationValues(int n)
    {
        int[] arr = new int[n + 1];

        for (int r = 0; r <= n; r++)
        {
            arr[r] = Combination(n, r);
        }
        return arr;
    }

    private int Combination(int n, int r)
    {
        if (n == r) return 1;
        if (r == 0) return 1;

        // C(n, r) == C(n, n - r)
        if (n - r < r)
            r = n - r;

        return Permutation(n, r) / Factorial(r);
    }

    private int Permutation(int n, int r)
    {
        if (r == 0) return 1;
        if (r == 1) return n;

        int result = n;
        int end = n - r + 1;
        for (int i = n - 1; i >= end; i--)
        {
            result *= i;
        }
        return result;
    }


    private int Factorial(int n)
    {
        if (n == 0 || n == 1) return 1;
        if (n == 2) return 2;

        int result = n;
        for (int i = n - 1; i > 1; i--)
        {
            result *= i;
        }
        return result;
    }

    private void SavePrevious()
    {
        if (_prevLength != points.Length)
        {
            _prevLength = points.Length;
            _prevPositions = new Vector3[_prevLength];
        }

        for (int i = 0; i < _prevLength; i++)
        {
            _prevPositions[i] = points[i];
        }

        _prevSampleCount = samplePointCount;
    }

    #endregion

}
