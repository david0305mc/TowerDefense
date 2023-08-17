using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MEnemyObj : MBaseObj
{
    public int UID { get; private set;}
    [SerializeField] private int tid;
    [SerializeField] private UnityEngine.AI.NavMeshAgent agent;

    public int TID { get; }

    public DataManager.Character refData { get; set; }

    private System.Action getDamageAction;

    private void Awake()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }
    public void InitObject(int _uid, System.Action _getDamageAction)
    {
        UID = _uid;
        getDamageAction = _getDamageAction;
        refData = DataManager.Instance.GetCharacterData(TID);
        agent.isStopped = true;
    }

    public override void GetDamaged(int _damage)
    {
        getDamageAction?.Invoke();
    }
}
