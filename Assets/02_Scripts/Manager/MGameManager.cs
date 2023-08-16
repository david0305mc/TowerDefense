using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MGameManager : SingletonMono<MGameManager>
{
    [SerializeField] private List<GameObject> stageprefLists;
    [SerializeField] private ProjectileStraight projStraight;

    [SerializeField] private Transform objRoot;
    [SerializeField] private MHeroObj heroObjPref;
    [SerializeField] private GameObject boomPref;


    private Dictionary<int, MEnemyObj> enemyDic;
    private StageObject currStageObj;

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();
    }

    public static int GenerateUID()
    {
        return UserData.Instance.LocalData.uidSeed++;
    }
    public MEnemyObj GetEnemyObj(int _uid)
    {
        if (enemyDic.ContainsKey(_uid))
            return enemyDic[_uid];
        return null;
    }
    public int GetNearestEnemyObj(Vector3 srcPos)
    {
        int nearestObjUID = -1;
        float shortDist = float.MaxValue;
        foreach (var item in enemyDic)
        {
            float dist = Vector3.Distance(srcPos, item.Value.transform.position);

            if (dist < shortDist)
            {
                shortDist = dist;
                nearestObjUID = item.Value.UID;
            }
        }

        return nearestObjUID;
    }

    private void Start()
    {
        InitGame();
    }

    private void SpawnStage(int stage)
    {
        if (currStageObj != null)
        {
            Destroy(currStageObj.gameObject);
        }
        currStageObj = Instantiate(stageprefLists[stage], Vector3.zero, Quaternion.identity, objRoot).GetComponent<StageObject>();
    }
    private void InitGame()
    {
        SpawnStage(UserData.Instance.CurrStage);
        InitEnemies();
    }

    private void InitEnemies()
    {
        enemyDic = new Dictionary<int, MEnemyObj>();
        var enemies = currStageObj.enemyObjRoot.GetComponentsInChildren<MEnemyObj>();
        foreach (MEnemyObj enemyObj in enemies)
        {
            var data = UserData.Instance.AddEnemyData(enemyObj.TID);
            enemyObj.InitObject(data.uid, ()=> {

                // GetDamaged
                bool isDead = UserData.Instance.AttackToEnmey(data.uid, 1);
                if (isDead)
                {
                    RemoveEnemy(data.uid);
                }
                else
                {
                    enemyObj.SetHPBar(data.hp / (float)data.refData.hp);
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

    public void LauchProjectile(MHeroObj heroObj, int _enemyUID)
    {
        ProjectileStraight bullet = Lean.Pool.LeanPool.Spawn(projStraight, heroObj.transform.position, Quaternion.identity, objRoot);
        bullet.Shoot(enemyDic[_enemyUID], 1);
    }

    public void AddHero()
    {
        Vector3 spawnPos = currStageObj.heroSpawnPos.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
        var heroObj = Lean.Pool.LeanPool.Spawn(heroObjPref, spawnPos, Quaternion.identity, objRoot);
        heroObj.StartFSM();
    }

    public void NextStage()
    {
        UserData.Instance.CurrStage++;
        SpawnStage(UserData.Instance.CurrStage);
        InitEnemies();
        
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
