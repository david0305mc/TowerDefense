using MonsterLove.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitTestObj : MonoBehaviour
{
    public enum FSMStates
    {
        Idle,
        Move,
    }

    private Animator animator;
    private NavMeshAgent agent;
    private Vector3 target;
    public float speed = 5f;
    protected StateMachine<FSMStates, StateDriverUnity> fsm;
    public string state;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        fsm = new StateMachine<FSMStates, StateDriverUnity>(this);
    }

    private void Start()
    {
        StartFSM();
    }
    public void SetTargetPos(Vector3 _target)
    {
        target = _target;
        fsm.ChangeState(FSMStates.Move);
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
        agent.SetDestination(target);
        FlipRenderers(agent.velocity.x < 0);
        if (Vector2.Distance(target, transform.position) < 0.1f)
        {
            fsm.ChangeState(FSMStates.Idle);
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
