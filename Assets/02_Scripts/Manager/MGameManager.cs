using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;
using Game;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

public partial class MGameManager : SingletonMono<MGameManager>
{
    [SerializeField] private MCameraManager cameraManager;

    [SerializeField] private Transform objRoot;
    [SerializeField] private MainUI mainUI;
    [SerializeField] private WorldMap worldMap;
    [SerializeField] private List<TileData> tileDatas;
    [SerializeField] private GoldReewardObj goldRewardPrefab;
    [SerializeField] private EffectPeedback enemydieEffectPrefab;

    private Dictionary<int, MEnemyObj> enemyDic;
    private Dictionary<int, MHeroObj> heroDic;
    private StageObject currStageObj;
    private AsyncOperationHandle<GameObject> currStageOpHandler; 

    public List<GameObject> WayPoints => currStageObj.wayPointLists;

    private Dictionary<TileBase, TileData> dataFromTileMap;
    private GameConfig.GameState gameState;

    private CancellationTokenSource stageCts;
    private CancellationTokenSource spawnHeroCts;

    private int enemyBossUID;


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

    public void StartStage(int stageID)
    {
        stageCts = new CancellationTokenSource();
        UserData.Instance.AcquireGold.Value = 0;
        //UserData.Instance.LocalData.Stamina.Value -= ConfigTable.Instance.StageStartCost;
        mainUI.SetIngameUI();
        if (currStageObj != null)
        {
            Destroy(currStageObj.gameObject);
        }
        var stageInfo = DataManager.Instance.GetStageInfoData(stageID);
        worldMap.gameObject.SetActive(false);
        cameraManager.SetZoomAndSize(2, 20, -10, 25, -10, 25);
        UniTask.Create(async () =>
        {
            currStageOpHandler = Addressables.InstantiateAsync(stageInfo.prefabname, Vector3.zero, Quaternion.identity, objRoot);
            await currStageOpHandler;
            currStageObj = currStageOpHandler.Result.GetComponent<StageObject>();
            UserData.Instance.CurrStage = stageID;
            InitEnemies();
            SpawnAllHero();
            mainUI.SetStageUI();
        });
    }

    public void BackToHome()
    {
        mainUI.SetWorldUI();
        cameraManager.SetZoomAndSize(2, 7, -2, 9, -2, 6);
        worldMap.gameObject.SetActive(true);
        worldMap.UpdateWorld();
    }
    public void RetryStage()
    {
        StartStage(UserData.Instance.CurrStage);
    }

    public void NextStage()
    {
        int nextStage = UserData.Instance.CurrStage + 1;
        var stageInfo = DataManager.Instance.GetStageInfoData(nextStage);
        if (stageInfo == null)
        {
            PopupManager.Instance.ShowSystemOneBtnPopup("There is no Stage", "OK");
            return;
        }
        StartStage(UserData.Instance.CurrStage + 1);
    }

    public void RemoveStage()
    {
        if (!Addressables.ReleaseInstance(currStageOpHandler))
        {
            Destroy(currStageOpHandler.Result.gameObject);
        }
        RemoveAllBattleHero();
        RemoveAllProjectile();
        spawnHeroCts?.Cancel();
        stageCts?.Cancel();
        //ResourceManagerTest.Instance.UnloadUnusedAssetsImmediate().Forget();
    }

    private void InitGame()
    {
        gameState = GameConfig.GameState.MainUI;
        mainUI.InitTabGroup();
        cameraManager.SetTouchAction(() =>
        {
            if (gameState == GameConfig.GameState.MainUI)
            {
                GameObject obj = cameraManager.TryGetRayCastObject(Input.mousePosition, GameConfig.StageSlotLayerMask);
                if (obj != null)
                {
                    cameraManager.SetFollowObject(obj, () =>
                    {
                        WorldMapStageSlot stageSlot = obj.GetComponent<WorldMapStageSlot>();
                        mainUI.ShowStageInfo(stageSlot.stage, () =>
                        {
                            // startBtn
                            StartStage(stageSlot.stage);
                            mainUI.HideStageInfo();
                        });
                    });
                }
                else
                {
                    cameraManager.CancelFollowTarget();
                    mainUI.HideStageInfo();
                }
            }
        }, 
        ()=> {
            if (gameState == GameConfig.GameState.MainUI)
            {
                mainUI.HideStageInfo();
            }
        });
        worldMap.InitWorld();
        mainUI.SetWorldUI();
    }

    private void Start()
    {
        InitGame();
    }

    private void InitEnemies()
    {
        enemyBossUID = 0;
        enemyDic = new Dictionary<int, MEnemyObj>();
        var enemies = currStageObj.enemyObjRoot.GetComponentsInChildren<MEnemyObj>();
        foreach (MEnemyObj enemyObj in enemies)
        {
            UnitData data = UserData.Instance.AddEnemyData(enemyObj.TID);
            enemyObj.InitObject(data.uid, true, (_attackData) => {

                var heroObj = GetHeroObj(_attackData.attackerUID);
                if (heroObj == null)
                {
                    // To Do : ??
                    return;
                }    
                DoEnemyGetDamage(enemyObj, heroObj.transform.position, _attackData.attackerUID, _attackData.damage);
            });
            if (enemyObj.IsEnemyBoss)
            {
                enemyBossUID = enemyObj.UID;
            }
            enemyDic.Add(data.uid, enemyObj);
        }
        if (enemyBossUID == 0)
        {
            Debug.LogError("There is No Enemy Boss");
            enemyBossUID = enemyDic.First().Value.UID;
        }
    }

    private void DoEnemyGetDamage(MEnemyObj _enemyObj, Vector3 attackerPos, int _attackerUID, int _damage)
    {
        // GetDamaged
        bool isDead = UserData.Instance.AttackToEnmey(_enemyObj.UID, _damage);
        if (!UserData.Instance.isBattleHeroDead(_attackerUID))
        {
            DoAggroToHero(_enemyObj, _attackerUID);
        }
        
        if (isDead)
        {
            KillEnemy(_attackerUID, _enemyObj.UID);
        }
        else
        {
            var heroObj = GetHeroObj(_attackerUID);
            if (heroObj == null)
            {
                // To Do - missile 
                return;
            }
            _enemyObj.GetAttacked(attackerPos, heroObj.UnitData.refUnitGradeData.knockback);
        }
        UIMain.Instance.ShowDamageText(_enemyObj.transform.position, _damage);
    }

    private void DoHeroGetDamage(MHeroObj _heroObj, Vector3 attackerPos, int _attackerUID, int _damage)
    {
        bool isDead = UserData.Instance.AttackToHero(_heroObj.UID, _damage);
        if (!UserData.Instance.IsEnemyDead(_attackerUID))
        {
            DoAggroToEnemy(_heroObj, _attackerUID);
        }
        if (isDead)
        {
            KillBattleHero(_attackerUID, _heroObj.UID);
        }
        else
        {
            var enemyObj = GetEnemyObj(_attackerUID);
            if (enemyObj == null)
            {
                // To Do : missile
                return;
            }
            _heroObj.GetAttacked(attackerPos, enemyObj.UnitData.refUnitGradeData.knockback);
        }
        UIMain.Instance.ShowDamageText(_heroObj.transform.position, _damage);
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

    private void KillBattleHero(int _attackerUID, int _uid)
    {
        UserData.Instance.KillBattleHero(_attackerUID, _uid);
        if (UserData.Instance.isAllHeroDead())
        {
            LoseStage();
        }
        if (heroDic.ContainsKey(_uid))
        {
            Lean.Pool.LeanPool.Despawn(heroDic[_uid].gameObject);
            heroDic.Remove(_uid);
        }
    }
    private void RemoveBattleHero(int _uid)
    {
        if (_uid == -1)
            return;
        UserData.Instance.RemoveBattleHero(_uid);
        if (heroDic.ContainsKey(_uid))
        {
            Lean.Pool.LeanPool.Despawn(heroDic[_uid].gameObject);
            heroDic.Remove(_uid);
        }
    }

    private void RemoveAllBattleHero()
    {
        for (int i = UserData.Instance.battleHeroDataDic.Count - 1; i >= 0; i--)
        {
            RemoveBattleHero(UserData.Instance.battleHeroDataDic.ElementAt(i).Key);
        }
    }

    private void KillEnemy(int attackerUID, int _uid)
    {
        UserData.Instance.KillEnemy(attackerUID, _uid);
        if (enemyDic.ContainsKey(_uid))
        {
            var enemyObj = enemyDic[_uid];
            var effectPeedback = Lean.Pool.LeanPool.Spawn(enemydieEffectPrefab, enemyObj.transform.position, Quaternion.identity, objRoot);
            effectPeedback.SetData(() =>
            {
                Lean.Pool.LeanPool.Despawn(effectPeedback);
                var goldObj = Lean.Pool.LeanPool.Spawn(goldRewardPrefab, enemyObj.transform.position, Quaternion.identity, objRoot);
                goldObj.Shoot(mainUI.GetUIStage.GoldTarget, () => {
                    UserData.Instance.AcquireGold.Value += enemyObj.UnitData.refUnitGradeData.dropgoldcnt;
                    if (_uid == enemyBossUID)
                    {
                        WinStage();
                    }
                });
            }, stageCts);
            enemyObj.gameObject.SetActive(false);
        }
    }

    private void RemoveEnemy(int _uid)
    {
        UserData.Instance.RemoveEnmey(_uid);
        if (enemyDic.ContainsKey(_uid))
        {
            Destroy(enemyDic[_uid].gameObject);
            enemyDic.Remove(_uid);
        }
    }
    private void RemoveAllEnemy()
    {
        for (int i = UserData.Instance.enemyDataDic.Count - 1; i >= 0; i--)
        {
            RemoveEnemy(UserData.Instance.enemyDataDic.ElementAt(i).Key);
        }
    }
    private void RemoveAllProjectile()
    {
        Lean.Pool.LeanPool.DespawnAll();
    }
    public void LauchProjectile(MBaseObj attackerObj, int _targetUID)
    {
        var projectileInfo = DataManager.Instance.GetProjectileInfoData(attackerObj.UnitData.refUnitGradeData.projectileid);
        ProjectileBase bullet = Lean.Pool.LeanPool.Spawn(MResourceManager.Instance.GetProjectile(projectileInfo.prefabname), attackerObj.FirePos, Quaternion.identity, objRoot);
        bullet.Shoot(new AttackData(attackerObj.UID, attackerObj.UnitData.tid, attackerObj.UnitData.refUnitGradeData.attackdmg, !attackerObj.UnitData.IsEnemy), GetUnitObj(_targetUID, !attackerObj.UnitData.IsEnemy), projectileInfo.speed);
    }

    private void SpawnHero(int unitUid)
    {
        var heroData = UserData.Instance.GetHeroData(unitUid);
        var battleHeroData = UserData.Instance.AddBattleHeroData(heroData);
        Vector3 spawnPos = currStageObj.heroSpawnPos.position + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0);

        GameObject unitPrefab = MResourceManager.Instance.GetPrefab(battleHeroData.refData.prefabname);
        MHeroObj heroObj = Lean.Pool.LeanPool.Spawn(unitPrefab, spawnPos, Quaternion.identity, objRoot).GetComponent<MHeroObj>();
        heroObj.InitObject(battleHeroData.uid, false, (_attackData) =>
        {
            var enemyObj = GetEnemyObj(_attackData.attackerUID);
            if (enemyObj == null)
            {
                // To Do :
                return;
            }
            DoHeroGetDamage(heroObj, enemyObj.transform.position, _attackData.attackerUID, _attackData.damage);
        });
        heroObj.StartFSM();
        heroDic.Add(battleHeroData.uid, heroObj);
    }

    private void SpawnAllHero()
    {
        heroDic = new Dictionary<int, MHeroObj>();
        spawnHeroCts?.Cancel();
        spawnHeroCts = new CancellationTokenSource();
        UniTask.Create(async () =>
        {
            await UniTask.Delay(1000, cancellationToken: spawnHeroCts.Token);
            foreach (var item in UserData.Instance.LocalData.BattlePartyDic)
            {
                if (item.Value != -1)
                {
                    var heroData = UserData.Instance.GetHeroData(item.Value);
                    for (int i = 0; i < heroData.refUnitGradeData.summoncnt; i++)
                    {
                        SpawnHero(item.Value);
                        await UniTask.Delay(300, cancellationToken: spawnHeroCts.Token);
                    }
                }
            }
        });
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
        return 3f;
        Vector3Int gridPosition = currStageObj.tileMap.WorldToCell(new Vector3(worldPosition.x, worldPosition.y, 0) );
        TileBase tilebase = currStageObj.tileMap.GetTile(gridPosition);
        if (tilebase == null)
            return 1f;
        //float walkingSpeed = dataFromTileMap[tilebase].walkingSpeed;
        //return walkingSpeed;
    }
    public void DoAreaAttack(AttackData _attackData, Vector2 _pos)
    {
        var unitGradeInfo = DataManager.Instance.GetUnitGrade(_attackData.attackerTID, 1);

        if (unitGradeInfo.splashrange == 0)
        {
            return;
        }

        var detectedObjs = Physics2D.OverlapCircleAll(_pos, unitGradeInfo.splashrange, Game.GameConfig.UnitLayerMask);
        if (detectedObjs.Length > 0)
        {
            foreach (var obj in detectedObjs)
            {
                if (_attackData.attackToEnemy)
                {
                    MEnemyObj enemyObj = obj.GetComponent<MEnemyObj>();
                    if (enemyObj != null)
                    {
                        DoEnemyGetDamage(enemyObj, _pos, _attackData.attackerUID, unitGradeInfo.splashdmg);
                    }
                }
                else
                {
                    MHeroObj heroObj = obj.GetComponent<MHeroObj>();
                    if (heroObj != null)
                    {
                        DoHeroGetDamage(heroObj, _pos, _attackData.attackerUID, unitGradeInfo.splashdmg);
                    }
                }
            }
        }

    }

    public void ShowBoomEffect(AttackData _attackData, Vector2 _pos, string name = default)
    {
        var unitGradeInfo = DataManager.Instance.GetUnitGrade(_attackData.attackerTID, 1);

        if (!string.IsNullOrEmpty(unitGradeInfo.boomeffectprefab))
        {
            ExplosionEffect effect = Lean.Pool.LeanPool.Spawn(MResourceManager.Instance.GetPrefab(unitGradeInfo.boomeffectprefab), _pos, Quaternion.identity, objRoot).GetComponent<ExplosionEffect>();
            effect.SetData(_attackData, () =>
            {
                Lean.Pool.LeanPool.Despawn(effect);
            });
        }
    }

    private void OnDestroy()
    {
        spawnHeroCts?.Cancel();
        spawnHeroCts?.Dispose();
        stageCts?.Cancel();
        stageCts?.Dispose();
    }
}
