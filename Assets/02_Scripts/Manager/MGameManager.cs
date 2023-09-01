using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class MGameManager : SingletonMono<MGameManager>
{
    [SerializeField] private List<GameObject> stageprefLists;

    [SerializeField] private Transform objRoot;
    [SerializeField] private List<MHeroObj> heroObjPrefList;
    [SerializeField] private List<ExplosionEffect> explosionPrefList;

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

    public MBaseObj GetUnitObj(int _uid, bool isEnemy)
    {
        if (isEnemy)
        {
            return GetEnemyObj(_uid);
        }
        return GetHeroObj(_uid);
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
            enemyObj.InitObject(data.uid, true, (_attackData) => {
                DoEnemyGetDamage(enemyObj, _attackData);
            });
            //enemyObj.transform.SetPosition(new Vector3(enemyObj.transform.position.x, enemyObj.transform.position.y, 0));
            enemyDic.Add(data.uid, enemyObj);
        }
    }

    private void DoEnemyGetDamage(MEnemyObj _enemyObj, AttackData _attackData)
    {
        // GetDamaged
        bool isDead = UserData.Instance.AttackToEnmey(_enemyObj.UID, _attackData.damage);
        if (UserData.Instance.GetHeroData(_attackData.attackerUID) != null)
        {
            DoAggroToHero(_enemyObj, _attackData.attackerUID);
        }
        
        if (isDead)
        {
            RemoveEnemy(_enemyObj.UID);
        }
        else
        {
            _enemyObj.DoFlashEffect();
            _enemyObj.UpdateHPBar();
        }
        UIMain.Instance.ShowDamageText(_enemyObj.transform.position, _attackData.damage);
    }

    private void DoHeroGetDamage(MHeroObj _heroObj, AttackData _attackData)
    {
        bool isDead = UserData.Instance.AttackToHero(_heroObj.UID, _attackData.damage);
        if (UserData.Instance.GetEnemyData(_attackData.attackerUID) != null)
        {
            DoAggroToEnemy(_heroObj, _attackData.attackerUID);
        }
        if (isDead)
        {
            RemoveHero(_heroObj.UID);
        }
        else
        {
            _heroObj.DoFlashEffect();
            _heroObj.UpdateHPBar();
        }
        UIMain.Instance.ShowDamageText(_heroObj.transform.position, _attackData.damage);
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
                    if (heroObj.State == MHeroObj.FSMStates.Idle || heroObj.State == MHeroObj.FSMStates.WaypointMove || heroObj.State == MHeroObj.FSMStates.DashMove)
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
                    if (enemyObj.State == MEnemyObj.FSMStates.Idle || enemyObj.State == MEnemyObj.FSMStates.DashMove)
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

    public void LauchProjectile(MBaseObj attackerObj, int _targetUID)
    {
        var projectileInfo = DataManager.Instance.GetProjectileInfoData(attackerObj.UnitData.refUnitGradeData.projectileid);
        ProjectileBase bullet = Lean.Pool.LeanPool.Spawn(MResourceManager.Instance.GetProjectile(projectileInfo.prefabname), attackerObj.FirePos, Quaternion.identity, objRoot);
        bullet.Shoot(new AttackData(attackerObj.UID, attackerObj.UnitData.tid, attackerObj.UnitData.refUnitGradeData.attackdmg, !attackerObj.UnitData.IsEnemy), GetUnitObj(_targetUID, !attackerObj.UnitData.IsEnemy), 1);
    }

    public void AddHero(int index)
    {   
        var heroData = UserData.Instance.AddHeroData(heroObjPrefList[index].TID);
        Vector3 spawnPos = currStageObj.heroSpawnPos.position + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0);
        MHeroObj heroObj = Lean.Pool.LeanPool.Spawn(heroObjPrefList[index], spawnPos, Quaternion.identity, objRoot);
        heroObj.InitObject(heroData.uid, false, (_attackData) =>
        {
            DoHeroGetDamage(heroObj, _attackData);
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
    public void DoAreaAttack(AttackData _attackData, Vector2 _pos)
    {
        var detectedObjs = Physics2D.OverlapCircleAll(_pos, 2, Game.GameConfig.UnitLayerMask);
        if (detectedObjs.Length > 0)
        {
            foreach (var obj in detectedObjs)
            {
                if (_attackData.attackToEnemy)
                {
                    MEnemyObj enemyObj = obj.GetComponent<MEnemyObj>();
                    if (enemyObj != null)
                    {
                        DoEnemyGetDamage(enemyObj, _attackData);
                    }
                }
                else
                {
                    MHeroObj heroObj = obj.GetComponent<MHeroObj>();
                    if (heroObj != null)
                    {
                        DoHeroGetDamage(heroObj, _attackData);
                    }
                }
            }
        }

    }

    public void ShowBoomEffect(int boomIndex, AttackData _attackData, Vector2 _pos, string name = default)
    {
        ExplosionEffect effect = Lean.Pool.LeanPool.Spawn(explosionPrefList[boomIndex], _pos, Quaternion.identity, objRoot);
        effect.SetData(_attackData, () =>
        {
            Lean.Pool.LeanPool.Despawn(effect);
        });
    }

}
