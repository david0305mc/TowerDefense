using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierMissile2D : MonoBehaviour
{
    [Range(0.01f, 1f)]
    public float gizmoRadius = 0.3f;

    [Range(0f, 1f)]
    public float progression = 0f;

    private Transform startTr;
    private Transform endTr;
    private Transform secondTr;
    public Transform[] points;

    public float elpase;

    [Space, Range(2, 100)]
    public int samplePointCount = 100;

    // Check Changed
    private int _prevLength = 0;
    private int _prevSampleCount = 0;
    private Vector3[] _prevPositions;


    // curve
    private Vector3[] curvePoints;

    private Rigidbody2D rigidBody2d;
    private Vector3 prevPos;

    private void Awake()
    {
        rigidBody2d = GetComponent<Rigidbody2D>();
    }
    public void Init(Transform _startTr, Transform _secondTR,  Transform _endTr, float _speed, float _newPointDistanceFromStartTr, float _newPointDistanceFromEndTr)
    {
        elpase = 0f;
        startTr = _startTr;
        endTr = _endTr;
        secondTr = _secondTR;
        transform.position = _startTr.position;
        
        points = new[] { startTr, secondTr, endTr };
        CalculateCurvePoints(samplePointCount);
        SavePrevious();
        Update();
    }

    //private void Start()
    //{
    //    secondTr.position = startTr.TransformPoint(new Vector2(-1.2f, -0.5f));
    //    points = new[] { startTr, secondTr, endTr };
    //    CalculateCurvePoints(samplePointCount);
    //    SavePrevious();
    //}

    //private void OnDrawGizmos()
    //{
    //    if (PointsChanged())
    //        CalculateCurvePoints(samplePointCount);

    //    Gizmos.color = Color.yellow;
    //    DrawCurve();

    //    SavePrevious();
    //}
    private void Update()
    {
        elpase += Time.deltaTime;

        if (elpase >= 1f)
        {

            Lean.Pool.LeanPool.Despawn(gameObject);
            return;
        }
        
        if (PointsChanged())
            CalculateCurvePoints(samplePointCount);

        //transform.position = Vector2.Lerp(startTr.position, endTr.position, elpase);

        float fLen = (curvePoints.Length - 1) * elpase;
        fLen = Mathf.Clamp((int)fLen, 0, curvePoints.Length - 1);
        var pos = curvePoints[(int) fLen];
        
        rigidBody2d.MovePosition(pos);
        //if (prevPos != pos)
        //{
        //    rigidBody2d.MoveRotation(GameUtil.LookAt2D(prevPos, pos, GameUtil.FacingDirection.RIGHT));
        //}
        
        prevPos = pos;
        SavePrevious();
    }
    private bool PointsChanged()
    {
        if (points == null || _prevPositions == null || points.Length == 0) return false;

        if (_prevLength != points.Length)
            return true;

        if (_prevSampleCount != samplePointCount)
            return true;

        for (int i = 0; i < points.Length && i < _prevPositions.Length; i++)
        {
            if (Vector3.SqrMagnitude(points[i].position - _prevPositions[i]) > 0.01f)
                return true;
        }
        return false;
    }
    private void CalculateCurvePoints(int count)
    {
        if (points == null || points.Length < 2) return;

        curvePoints = new Vector3[count + 1];
        float unit = 1.0f / count;

        ref Transform[] P = ref points;

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
                curvePoints[k] += C[i] * T[i] * U[n - i] * P[i].position;
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


    private void DrawCurve()
    {
        if (points == null || points.Length < 2) return;
        if (curvePoints == null || curvePoints.Length < 2) return;

        float fLen = (curvePoints.Length - 1) * progression;
        int i = 0;
        for (; i < fLen; i++)
        {
            Gizmos.DrawLine(curvePoints[i], curvePoints[i + 1]);
        }
        Gizmos.DrawWireSphere(curvePoints[i], gizmoRadius * 0.8f);
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
            _prevPositions[i] = points[i].position;
        }

        _prevSampleCount = samplePointCount;
    }

}


