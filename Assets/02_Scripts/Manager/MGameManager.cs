using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MGameManager : SingletonMono<MGameManager>
{
    [SerializeField] private ProjectileStraight projStraight;

    [SerializeField] private Transform objRoot;
    [SerializeField] private Transform enemyObjRoot;
    [SerializeField] private MHeroObj testHeroObj;


    private Dictionary<int, MEnemyObj> enemyDic;

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();
    }

    public static int GenerateUID()
    {
        return UserData.Instance.LocalData.uidSeed++;
    }

    public MEnemyObj GetNearestEnemyObj(Vector3 srcPos)
    {
        MEnemyObj nearestObj = null;
        float shortDist = float.MaxValue;
        foreach (var item in enemyDic)
        {
            float dist = Vector3.Distance(srcPos, item.Value.transform.position);

            if (dist < shortDist)
            {
                shortDist = dist;
                nearestObj = item.Value;
            }
        }

        return nearestObj;
    }

    private void Start()
    {
        InitGame();
    }

    private void InitGame()
    {
        InitEnemies();
        testHeroObj.StartFSM();
    }

    private void InitEnemies()
    {
        enemyDic = new Dictionary<int, MEnemyObj>();
        var enemies = enemyObjRoot.GetComponentsInChildren<MEnemyObj>();
        foreach (var enemyObj in enemies)
        {
            var data = UserData.Instance.AddEnemyData(enemyObj.TID);
            enemyObj.InitObject(data.uid);
            enemyDic.Add(data.uid, enemyObj);
        }
    }

    public void RemoveEnemy(MEnemyObj targetObj)
    {
        Destroy(targetObj.gameObject);
        enemyDic.Remove(targetObj.UID);
    }

    public void LauchProjectile(MHeroObj heroObj, MEnemyObj enemyObj)
    {
        ProjectileStraight bullet = Lean.Pool.LeanPool.Spawn(projStraight, heroObj.transform.position, Quaternion.identity, objRoot);
        bullet.Shoot(enemyObj, 1);
    }

}
