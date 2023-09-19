using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TestDefeneder : MonoBehaviour
{
    
    public GameObject target;
    private NavMeshAgent agent;
    private Rigidbody2D rigidBody2d;
    private CancellationTokenSource cts;
    private void Awake()
    {
        cts = new CancellationTokenSource();
        rigidBody2d = GetComponent<Rigidbody2D>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        if (agent.isActiveAndEnabled)
        {
            if (Vector2.Distance(target.transform.position, transform.position) > 1f)
            {
                agent.SetDestination(new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z));
                FlipRenderers(transform.position.x > target.transform.position.x);
            }
            else
            {
                agent.velocity = Vector3.zero;
            }
        }
    }

    protected void FlipRenderers(bool value)
    {
        if (value)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void GetAttacked(GameObject sender)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        // Begin
        rigidBody2d.velocity = Vector3.zero;
        agent.enabled = false;
        Vector2 direction = (transform.position - sender.transform.position).normalized;
        rigidBody2d.AddForce(direction * 8, ForceMode2D.Impulse);
        // End
        UniTask.Create(async () =>
        {
            Debug.Log("UniTask.Create");
            //await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f), cancellationToken: cts.Token);
            await UniTask.Delay(150, cancellationToken: cts.Token);
            rigidBody2d.velocity = Vector3.zero;
            agent.enabled = true;
            Debug.Log("UniTask.Create End");
        });

    }
}
