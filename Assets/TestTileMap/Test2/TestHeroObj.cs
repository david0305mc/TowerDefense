using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TestHeroObj : MonoBehaviour
{

    private CancellationTokenSource cts;
    public void MoveTo(Vector3 targetPos)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        UniTask.Create(async () => {

            while (true)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
                if (Vector2.Distance(transform.position, targetPos) < 0.1f)
                {
                    cts.Cancel();
                    break;
                }
                await UniTask.WaitForFixedUpdate(cancellationToken: cts.Token);
            }
        });
    }
    public void MoveTo(TestGroundManager.Path _path)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        UniTask.Create(async () => {

            int pathIndex = 0;
            while (true)
            {
                try
                {
                    var targetWorldPos = MapMangerTest.Instance.tileMap.GetCellCenterWorld(new Vector3Int((int)_path.nodes[pathIndex].x, (int)_path.nodes[pathIndex].z, 0));

                    transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * 10f);
                    //if (Vector2.Distance(transform.position, targetWorldPos) < 0.1f)
                    //{
                    //    pathIndex++;
                    //    transform.position = targetWorldPos;
                    //    if (pathIndex >= _path.nodes.Length)
                    //    {
                    //        cts?.Cancel();
                    //        break;
                    //    }
                    //}
                    await UniTask.WaitForFixedUpdate(cancellationToken: cts.Token);
                }
                catch
                {
                    Debug.LogError($"pathIndex {pathIndex}");
                }

            }
        });
    }
}
