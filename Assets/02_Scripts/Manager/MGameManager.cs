using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MGameManager : SingletonMono<MGameManager>
{
    [SerializeField] private List<GameObject> stageprefLists;
    [SerializeField] private ProjectileStraight projStraight;

    [SerializeField] private Transform objRoot;
    [SerializeField] private List<MHeroObj> heroObjPrefList;
    [SerializeField] private List<GameObject> boomPrefList;

    private Dictionary<int, MEnemyObj> enemyDic;
    private Dictionary<int, MHeroObj> heroDic;
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
        InitHeroes();
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

    private void InitHeroes()
    {
        heroDic = new Dictionary<int, MHeroObj>();
    }

    public void RemoveEnemy(int _uid)
    {
        UserData.Instance.RemoveEnmey(_uid);
        Destroy(enemyDic[_uid].gameObject);
        enemyDic.Remove(_uid);
    }

    public void LauchProjectile(MHeroObj heroObj, int _enemyUID)
    {
        ProjectileStraight bullet = Lean.Pool.LeanPool.Spawn(projStraight, heroObj.transform.position, Quaternion.identity, objRoot);
        bullet.Shoot(enemyDic[_enemyUID], 1);
    }

    public void AddHero(int index)
    {   
        var heroData = UserData.Instance.AddHeroData(heroObjPrefList[index].TID);
        Vector3 spawnPos = currStageObj.heroSpawnPos.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
        MHeroObj heroObj = Lean.Pool.LeanPool.Spawn(heroObjPrefList[index], spawnPos, Quaternion.identity, objRoot);
        heroObj.InitObject(heroData.uid, () =>
        {

        });
        heroObj.StartFSM();
        heroDic.Add(heroData.uid, heroObj);
    }

    public void NextStage()
    {
        if (stageprefLists.Count <= UserData.Instance.CurrStage + 1)
        {
            return;
        }
        UserData.Instance.CurrStage++;
        SpawnStage(UserData.Instance.CurrStage);

        for (int i = UserData.Instance.enemyDataDic.Count - 1; i >= 0; i--)
        {
            RemoveEnemy(UserData.Instance.enemyDataDic.ElementAt(i).Key);
        }
        
        InitEnemies();
        
    }
    public void ShowBoomEffect(int boomIndex, Vector2 _pos, string name = default)
    {
        var boomEffect = Instantiate(boomPrefList[boomIndex]);
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
