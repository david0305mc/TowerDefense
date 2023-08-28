using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierShooter2D : MonoBehaviour
{
    public BezierMissile2D m_missilePrefab; // �̻��� ������.
    public GameObject m_target; // ���� ����.
    public Transform secondTR;
    public Vector2 secondTROffset;

    [Header("�̻��� ��� ����")]
    public float m_speed = 2; // �̻��� �ӵ�.
    [Space(10f)]
    public float m_distanceFromStart = 6.0f; // ���� ������ �������� �󸶳� ������.
    public float m_distanceFromEnd = 3.0f; // ���� ������ �������� �󸶳� ������.
    [Space(10f)]
    public int m_shotCount = 12; // �� �� �� �߻��Ұ���.
    [Range(0, 1)] public float m_interval = 0.15f;
    public int m_shotCountEveryInterval = 2; // �ѹ��� �� ���� �߻��Ұ���.


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