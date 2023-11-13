using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GravityMissileTest : MonoBehaviour
{

    public void Shoot(Vector3 endPos, float jumpSpeed, float gravity)
    {
        Vector3 start = transform.position; //���� ��ġ
        Vector3 end = endPos; //������ ��ġ

        float jumpTime = Mathf.Max(0.3f, Vector3.Distance(start, end) / jumpSpeed); //�Ÿ����� �ӷ��� �ؼ� �ð��� �� (��,��,��)
        float currentTime = 0f;
        float percent = 0f;
        //y������ �ʱ�ӵ�
        float v0 = (end - start).y - gravity;


        UniTask.Create(async () =>
        {

            while (percent < 1)
            {
                currentTime += Time.deltaTime;
                percent = currentTime / jumpTime; //percent�� currentTime == jumpTime�� �Ǿ�� 1�� �ȴ�

                Vector3 position = Vector3.Lerp(start, end, percent); //�ڿ������� �̵��ϱ� ���� Lerp���, percent��ŭ �̵�

                //������ � : ������ġ + �ʱ�ӵ�*�ð� + �߷�*�ð�����
                position.y = start.y + (v0 * percent) + (gravity * percent * percent);

                transform.position = position;
                await UniTask.Yield();
            }
        });
    }
}
