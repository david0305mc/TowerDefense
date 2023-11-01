using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParabolicTrajectory : MonoBehaviour
{
    public float initialSpeed = 10f;     // 초기 속도
    public float angle = 45f;            // 발사 각도

    private void Start()
    {
        DrawParabolicTrajectory();
    }

    private void Update()
    {
        DrawParabolicTrajectory();
    }
    void DrawParabolicTrajectory()
    {
        // 초기 속도와 발사 각도를 라디안으로 변환합니다.
        float radians = angle * Mathf.Deg2Rad;

        // 초기 속도를 x, y 성분으로 분리합니다.
        float initialVelocityX = initialSpeed * Mathf.Cos(radians);
        float initialVelocityY = initialSpeed * Mathf.Sin(radians);

        // 중력 가속도를 가져옵니다.
        float gravity = Mathf.Abs(Physics.gravity.y);

        // 시간 간격
        float timeStep = 0.02f;

        // 초기 위치 설정
        Vector3 currentPosition = transform.position;

        // 포물선 궤적 그리기
        for (float t = 0; t < 10f; t += timeStep)
        {
            float x = initialVelocityX * t;
            float y = (initialVelocityY * t) - (0.5f * gravity * t * t);

            // 현재 시간에 따른 위치 계산
            Vector3 newPosition = new Vector3(x, y, 0f);

            // 궤적 선 그리기
            Debug.DrawLine(currentPosition, currentPosition + newPosition, Color.red, 0.1f);

            // 현재 위치 갱신
            currentPosition = newPosition;
        }
    }
}