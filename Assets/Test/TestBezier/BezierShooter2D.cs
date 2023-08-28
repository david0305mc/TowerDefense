using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierShooter2D : MonoBehaviour
{
    public BezierMissile2D m_missilePrefab; // 미사일 프리팹.
    public GameObject m_target; // 도착 지점.
    public Transform secondTR;
    public Vector2 secondTROffset;

    [Header("미사일 기능 관련")]
    public float m_speed = 2; // 미사일 속도.
    [Space(10f)]
    public float m_distanceFromStart = 6.0f; // 시작 지점을 기준으로 얼마나 꺾일지.
    public float m_distanceFromEnd = 3.0f; // 도착 지점을 기준으로 얼마나 꺾일지.
    [Space(10f)]
    public int m_shotCount = 12; // 총 몇 개 발사할건지.
    [Range(0, 1)] public float m_interval = 0.15f;
    public int m_shotCountEveryInterval = 2; // 한번에 몇 개씩 발사할건지.


    private void Update()
    {
        transform.position = transform.position;
        transform.rotation = GameUtil.LookAt2D(transform.position, m_target.transform.position, GameUtil.FacingDirection.UP);
        secondTR.position = transform.TransformPoint(secondTROffset);
        if (Input.GetMouseButtonDown(0))
        {
            for (int i = 0; i < 3; i++)
            {
                Debug.Log("KeyCode.A");
                BezierMissile2D missile = Lean.Pool.LeanPool.Spawn(m_missilePrefab);
                missile.transform.position = this.gameObject.transform.position;
                missile.GetComponent<BezierMissile2D>().Init(this.gameObject.transform, secondTR, m_target.transform, m_speed, m_distanceFromStart, m_distanceFromEnd);

            }
        }
    }

}