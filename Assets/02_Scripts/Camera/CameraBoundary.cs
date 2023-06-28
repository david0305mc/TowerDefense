using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBoundary : SingletonMono<CameraBoundary>
{
    [SerializeField] private Rect bound;

    private Vector3 cameraClampTopLeftPosition;
    public Vector3 CameraClampTopLeftPosition { get { return cameraClampTopLeftPosition; } }

    private Vector3 cameraClampBottomRightBotPosition;
    public Vector3 CameraClampBottomRightBotPosition { get { return cameraClampBottomRightBotPosition; } }

    public Rect Bound
    {
        get
        {
            return bound;
        }
        set
        {
            bound = value;
            //_UpdateBoundPositions();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(Bound.width, Bound.height, 0));
    }

    private void LateUpdate()
    {
        if (transform.hasChanged)
        {
            UpdateBoundPositions();
            transform.hasChanged = false;
        }
    }
    private void UpdateBoundPositions()
    {
        Vector3 delta = transform.TransformVector(new Vector3(-Bound.width * 0.5f, bound.height * 0.5f, 0));
        cameraClampTopLeftPosition = transform.position + delta;
        cameraClampBottomRightBotPosition = transform.position - delta;
    }

    private void OnValidate()
    {
        UpdateBoundPositions();
    }
}
