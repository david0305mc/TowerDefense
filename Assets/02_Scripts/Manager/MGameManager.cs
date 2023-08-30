using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class MGameManager : SingletonMono<MGameManager>
{
    [SerializeField] private List<GameObject> stageprefLists;
    [SerializeField] private ProjectileStraight projStraight;

    [SerializeField] private Transform objRoot;
    [SerializeField] private List<MHeroObj> heroObjPrefList;
    [SerializeField] private List<CartoonFX.CFXR_Effect> boomPrefList;
    [SerializeField] private CartoonFX.CFXR_Effect effectPref;

    private Dictionary<int, MEnemyObj> enemyDic;
    private Dictionary<int, MHeroObj> heroDic;
    private StageObject currStageObj;

    public List<GameObject> WayPoints => currStageObj.wayPointLists;

    [SerializeField] private List<TileData> tileDatas;
    private Dictionary<TileBase, TileData> dataFromTileMap;

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();

        dataFromTileMap = new Dictionary<TileBase, TileData>();
        foreach (var tileData in tileDatas)
        {
            foreach (var tile in tileData.tiles)
            {
                dataFromTileMap.Add(tile, tileData);
            }
        }
    }

    public static int GenerateUID()
    {
        return UserData.Instance.LocalData.uidSeed++;
    }

    public MHeroObj GetHeroObj(int _uid)
    {
        if (heroDic.TryGetValue(_uid, out MHeroObj heroObj))
        {
            return heroObj;
        }
        return default;
    }
    public MEnemyObj GetEnemyObj(int _uid)
    {
        if (enemyDic.TryGetValue(_uid, out MEnemyObj enemyObj))
            return enemyObj;
        return default;
    }
    public int GetNearestEnemyObj(Vector3 srcPos, List<int> _blackList)
    {
        int nearestObjUID = -1;
        float shortDist = float.MaxValue;
        foreach (var item in enemyDic)
        {
            if (_blackList.Contains(item.Key))
                continue;

            float dist = Vector3.Distance(srcPos, item.Value.transform.position);

            if (dist < shortDist)
            {
                shortDist = dist;
                nearestObjUID = item.Value.UID;
            }
        }

        return nearestObjUID;
    }

    private void OnEnable()
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
            UnitData data = UserData.Instance.AddEnemyData(enemyObj.TID);
            enemyObj.InitObject(data.uid, (_attackData)=> {

                // GetDamaged
                bool isDead = UserData.Instance.AttackToEnmey(data.uid, _attackData.damage);
                DoAggroToHero(enemyObj, _attackData.attackerUID);

                if (isDead)
                {
                    RemoveEnemy(data.uid);
                }
                else
                {
                    enemyObj.DoFlashEffect();
                    enemyObj.SetHPBar(data.hp / (float)data.refUnitGradeData.hp);
                }
                UIMain.Instance.ShowDamageText(enemyObj.transform.position, _attackData.damage);

            });
            enemyDic.Add(data.uid, enemyObj);
        }
    }

    private void DoAggroToEnemy(MHeroObj _heroObj, int _attackerUID)
    {
        var detectedObjs = Physics2D.OverlapCircleAll(_heroObj.transform.position, 5, Game.GameConfig.UnitLayerMask);
        if (detectedObjs.Length > 0)
        {
            foreach (var obj in detectedObjs)
            {
                MHeroObj heroObj = obj.GetComponent<MHeroObj>();
                if (heroObj != null)
                {
                    if (heroObj == this)
                        continue;

                    if (heroObj.State == MHeroObj.FSMStates.Idle)
                    {
                        heroObj.DoAggro(_attackerUID);
                    }
                }
            }
        }
    }

    private void DoAggroToHero(MEnemyObj _enemyObj, int _attackerUID) 
    {
        var detectedObjs = Physics2D.OverlapCircleAll(_enemyObj.transform.position, 5, Game.GameConfig.UnitLayerMask);
        if (detectedObjs.Length > 0)
        {
            foreach (var obj in detectedObjs)
            {
                MEnemyObj enemyObj = obj.GetComponent<MEnemyObj>();
                if (enemyObj != null)
                {
                    if (enemyObj == this)
                        continue;

                    if (enemyObj.State == MEnemyObj.FSMStates.Idle)
                    {
                        enemyObj.DoAggro(_attackerUID);
                    }
                }
            }
        }
    }

    private void InitHeroes()
    {
        heroDic = new Dictionary<int, MHeroObj>();
    }

    private void RemoveHero(int _uid)
    {
        UserData.Instance.RemoveHero(_uid);
        Lean.Pool.LeanPool.Despawn(heroDic[_uid].gameObject);
        heroDic.Remove(_uid);
    }
    public void RemoveEnemy(int _uid)
    {
        UserData.Instance.RemoveEnmey(_uid);
        Destroy(enemyDic[_uid].gameObject);
        enemyDic.Remove(_uid);
    }

    public void LauchProjectileToHero(MBaseObj enemyObj, int _heroUID)
    {
        var projectileInfo = DataManager.Instance.GetProjectileInfoData(enemyObj.UnitData.refUnitGradeData.projectileid);
        ProjectileBase bullet = Lean.Pool.LeanPool.Spawn(MResourceManager.Instance.GetProjectile(projectileInfo.prefabname), enemyObj.FirePos, Quaternion.identity, objRoot);
        bullet.Shoot(new AttackData(enemyObj.UID, enemyObj.UnitData.tid, enemyObj.UnitData.refUnitGradeData.attackdmg) , heroDic[_heroUID], 1);
    }

    public void LauchProjectileToEnemy(MBaseObj heroObj, int _enemyUID)
    {
        var projectileInfo = DataManager.Instance.GetProjectileInfoData(heroObj.UnitData.refUnitGradeData.projectileid);
        //var projectileInfo = DataManager.Instance.GetProjectileInfoData(2);
        ProjectileBase bullet = Lean.Pool.LeanPool.Spawn(MResourceManager.Instance.GetProjectile(projectileInfo.prefabname), heroObj.FirePos, Quaternion.identity, objRoot);
        bullet.Shoot(new AttackData(heroObj.UID, heroObj.UnitData.tid, heroObj.UnitData.refUnitGradeData.attackdmg) , enemyDic[_enemyUID], 1);
    }

    public void AddHero(int index)
    {   
        var heroData = UserData.Instance.AddHeroData(heroObjPrefList[index].TID);
        Vector3 spawnPos = currStageObj.heroSpawnPos.position + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0);
        MHeroObj heroObj = Lean.Pool.LeanPool.Spawn(heroObjPrefList[index], spawnPos, Quaternion.identity, objRoot);
        heroObj.InitObject(heroData.uid, (_attackData) =>
        {
            // GetDamaged
            bool isDead = UserData.Instance.AttackToHero(heroData.uid, _attackData.damage);
            DoAggroToEnemy(heroObj, _attackData.attackerUID);

            if (isDead)
            {
                 RemoveHero(heroData.uid);
            }
            else
            {
                heroObj.SetHPBar(heroData.hp / (float)heroData.refUnitGradeData.hp);
            }
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

    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //        Vector3Int gridPosition = currStageObj.tileMap.WorldToCell(mousePosition);

    //        TileBase clickedTile = currStageObj.tileMap.GetTile(gridPosition);
    //        float walkingSpeed = dataFromTileMap[clickedTile].walkingSpeed;
    //        Debug.Log($"walkingSpeed {walkingSpeed}");
    //    }
    //}

    public float GetTileWalkingSpeed(Vector3 worldPosition) 
    {
        Vector3Int gridPosition = currStageObj.tileMap.WorldToCell(new Vector3(worldPosition.x, worldPosition.y, 0) );
        TileBase tilebase = currStageObj.tileMap.GetTile(gridPosition);
        if (tilebase == null)
            return 1f;

        float walkingSpeed = dataFromTileMap[tilebase].walkingSpeed;
        return walkingSpeed;
    }
    public void ShowBoomEffect(int boomIndex, Vector2 _pos, string name = default)
    {
        var effect = Lean.Pool.LeanPool.Spawn(boomPrefList[boomIndex]);
        effect.EndAction = () => {
            Lean.Pool.LeanPool.Despawn(effect);
        };
        effect.transform.position = _pos;
    }

}
