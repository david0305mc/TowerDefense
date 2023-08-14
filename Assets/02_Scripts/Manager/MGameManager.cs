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
    [SerializeField] private GameObject boomPref;


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
            enemyObj.InitObject(data.uid, ()=> {

                bool isDead = UserData.Instance.AttackToEnmey(data.uid, 1);
                if (isDead)
                {
                    RemoveEnemy(data.uid);
                }

            });
            enemyDic.Add(data.uid, enemyObj);
        }
    }

    public void RemoveEnemy(int _uid)
    {
        Destroy(enemyDic[_uid].gameObject);
        enemyDic.Remove(_uid);
    }

    public void LauchProjectile(MHeroObj heroObj, MEnemyObj enemyObj)
    {
        ProjectileStraight bullet = Lean.Pool.LeanPool.Spawn(projStraight, heroObj.transform.position, Quaternion.identity, objRoot);
        bullet.Shoot(enemyObj, 1);
    }

    public void ShowBoomEffect(Vector2 _pos, string name = default)
    {
        var boomEffect = Instantiate(boomPref);
        boomEffect.name = name;
        boomEffect.transform.position = _pos;

        boomEffect.gameObject.SetActive(false);
        boomEffect.gameObject.SetActive(true);
        //ParticleSystem ps = boomEffect.GetComponent<ParticleSystem>();
        //if (ps != null)
        //{
        //    var main = ps.main;
        //    if (main.loop)
        //    {
        //        ps.gameObject.AddComponent<CFX_AutoStopLoopedEffect>();
        //        ps.gameObject.AddComponent<CFX_AutoDestructShuriken>();
        //    }
        //}
    }

}
