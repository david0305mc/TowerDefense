using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GravityMissileTest : MonoBehaviour
{
    protected Rigidbody2D rigidBody2d;
    protected Vector2 prevPos;

    private void Awake()
    {
        rigidBody2d = GetComponent<Rigidbody2D>();
    }
    public void Shoot(Vector3 endPos, float jumpSpeed, float gravity)
    {
        Vector3 start = transform.position; //현재 위치
        Vector3 end = endPos; //끝나는 위치
        
        float jumpTime = Mathf.Max(0.3f, Vector3.Distance(start, end) / jumpSpeed); //거리분의 속력을 해서 시간을 얻어냄 (거,속,시)
        float currentTime = 0f;
        float percent = 0f;
        //y방향의 초기속도
        float v0 = (end - start).y - gravity;
        prevPos = transform.position;

        UniTask.Create(async () =>
        {

            while (percent < 1)
            {
                currentTime += Time.deltaTime;
                percent = currentTime / jumpTime; //percent는 currentTime == jumpTime이 되어야 1이 된다

                Vector3 position = Vector3.Lerp(start, end, percent); //자연스럽게 이동하기 위해 Lerp사용, percent만큼 이동

                //포물선 운동 : 시작위치 + 초기속도*시간 + 중력*시간제곱
                position.y = start.y + (v0 * percent) + (gravity * percent * percent);
                
                //transform.position = position;
                rigidBody2d.MovePosition(position);
                rigidBody2d.MoveRotation(GameUtil.LookAt2D(prevPos, position, GameUtil.FacingDirection.RIGHT));
                prevPos = position;
                await UniTask.Yield();
            }
            Lean.Pool.LeanPool.Despawn(gameObject);

        });
    }
}
