using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoTest : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0, 0, 1f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawSphere(Vector3.zero, 1f);
    }
    
}
