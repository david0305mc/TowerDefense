using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FieldOfViewAngle : MonoBehaviour
{
    private NavMeshAgent nav;

    //void Start()
    //{
    //    thePlayer = FindObjectOfType<PlayerController>();
    //    nav = GetComponent<NavMeshAgent>();
    //}

    //public bool View()
    //{
    //    Collider[] _target = Physics.OverlapSphere(transform.position, viewDistance, targetMask);

    //    for (int i = 0; i < _target.Length; i++)
    //    {
    //        Transform _targetTf = _target[i].transform;
    //        if (_targetTf.name == "Player")
    //        {
    //            Vector3 _direction = (_targetTf.position - transform.position).normalized;
    //            float _angle = Vector3.Angle(_direction, transform.forward);

    //            if (_angle < viewAngle * 0.5f)
    //            {
    //                RaycastHit _hit;
    //                if (Physics.Raycast(transform.position + transform.up, _direction, out _hit, viewDistance))
    //                {
    //                    if (_hit.transform.name == "Player")
    //                    {
    //                        Debug.Log("플레이어가 시야 내에 있습니다.");
    //                        Debug.DrawRay(transform.position + transform.up, _direction, Color.blue);

    //                        return true;
    //                    }
    //                }
    //            }
    //        }

    //        if (thePlayer.GetRun())
    //        {
    //            if (CalcPathLength(thePlayer.transform.position) <= viewDistance)
    //            {
    //                Debug.Log("주변에 뛰고 있는 플레이어의 움직임을 파악했습니다.");
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    //private float CalcPathLength(Vector3 _targetPos)
    //{
    //    NavMeshPath _path = new NavMeshPath();
    //    nav.CalculatePath(_targetPos, _path);

    //    Vector3[] _wayPoint = new Vector3[_path.corners.Length + 2];

    //    _wayPoint[0] = transform.position;
    //    _wayPoint[_path.corners.Length + 1] = _targetPos;

    //    float _pathLength = 0;  // 경로 길이를 더함
    //    for (int i = 0; i < _path.corners.Length; i++)
    //    {
    //        _wayPoint[i + 1] = _path.corners[i];
    //        _pathLength += Vector3.Distance(_wayPoint[i], _wayPoint[i + 1]);
    //    }

    //    return _pathLength;
    //}
}
