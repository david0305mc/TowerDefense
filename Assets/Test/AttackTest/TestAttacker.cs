using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAttacker : MonoBehaviour
{
    public enum FSMStates
    {
        Idle,
        Move,
        Attack,
    }

    private Animator animator;
    private UnityEngine.AI.NavMeshAgent agent;
    public TestDefeneder target;
    public float speed = 5f;
    protected StateMachine<FSMStates, StateDriverUnity> fsm;
    public string state;
    private float timeElapse;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
    }

    private void Start()
    {
        StartFSM();
    }

    private void Update()
    {
        fsm.Driver.Update.Invoke();
    }
    public virtual void StartFSM()
    {
        fsm.ChangeState(FSMStates.Idle);
    }

    protected virtual void Idle_Enter()
    {
        PlayAni("Idle");
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.speed = 0f;
        state = fsm.State.ToString();
    }
    protected virtual void Idle_Update()
    {
        if (Vector2.Distance(target.transform.position, transform.position) < 1f)
        {
            fsm.ChangeState(FSMStates.Attack);
        }
        else
        {
            fsm.ChangeState(FSMStates.Move);
        }
    }

    protected virtual void Move_Enter()
    {
        PlayAni("Walk");
        state = fsm.State.ToString();
        agent.speed = speed;
        agent.isStopped = false;
    }
    protected virtual void Move_Update()
    {
        agent.SetDestination(new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z));
        FlipRenderers(agent.velocity.x < 0);
        if (Vector2.Distance(target.transform.position, transform.position) < 1f)
        {
            fsm.ChangeState(FSMStates.Attack);
        }
    }

    protected virtual void Attack_Enter()
    {
        PlayAni("Attack");
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.speed = 0f;
        state = fsm.State.ToString();
        timeElapse = 0f;
    }
    protected virtual void Attack_Update()
    {
        timeElapse += Time.deltaTime;
        if (timeElapse >= 1f)
        {
            timeElapse = 0f;
            Debug.Log("Attack_Update");
            PlayAni("Attack");
            target.GetAttacked(gameObject);
        }
    }


    public void PlayAni(string str)
    {
        animator.Play(str);
        animator.Update(0);
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
}
