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
    [SerializeField] private InGameUI ingameUI;
    [SerializeField] private WorldMap worldMap;
    [SerializeField] private List<TileData> tileDatas;
    [SerializeField] private SoulRewardObj soulRewardPrefab;
    [SerializeField] private GoldRewardObj goldRewardPrefab;
    [SerializeField] private GameObject cameraFollowObject;

    private Dictionary<int, MEnemyObj> enemyDic;
    private Dictionary<int, MHeroObj> heroDic;
    private List<int> heroUIDOrder;
    private StageObject currStageObj;
    private AsyncOperationHandle<GameObject> currStageOpHandler; 

    public List<GameObject> WayPoints => currStageObj.wayPointLists;

    private Dictionary<TileBase, TileData> dataFromTileMap;
    private GameConfig.GameState gameState;

    private CancellationTokenSource followCameraCts;
    private CancellationTokenSource timerCts;
    private CancellationTokenSource stageCts;
    private CancellationTokenSource spawnHeroCts;

    private int enemyBossUID;
    public int EnemyBossUID => enemyBossUID;

    public static int flashUidSeed = 1000;
    private float cameraFollowTime;

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
    public static int GenerateFlashUID()
    {
        return flashUidSeed++;
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

    public void StartWaveStage()
    { 
         gameState = GameConfig.GameState.InGame_SpawningHero;
        stageCts = new CancellationTokenSource();
        timerCts = new CancellationTokenSource();
        UserData.Instance.AcquireSoul.Value = 0;
        //UserData.Instance.LocalData.Stamina.Value -= ConfigTable.Instance.StageStartCost;
         SetIngameUI();
        var waitTask = ingameUI.StartLoadingUI(); 
        if (currStageObj != null)
        {
            Destroy(currStageObj.gameObject);
        }
        var stageInfo = DataManager.Instance.GetStageInfoData(Game.GameConfig.waveStage);
        worldMap.gameObject.SetActive(false);
        UniTask.Create(async () =>
        {
            currStageOpHandler = Addressables.InstantiateAsync(stageInfo.prefabname, Vector3.zero, Quaternion.identity, objRoot);
            await currStageOpHandler;
            await waitTask;
            currStageObj = currStageOpHandler.Result.GetComponent<StageObject>();
            UserData.Instance.PlayingStage = Game.GameConfig.waveStage;
            ingameUI.EndLoadingUI();
            SetStageUI(timerCts);
            InitEnemies();
            InitInGameSpeed();
            InitFollowCamera().Forget();
            await SpawnAllHero();
            StartEnemyWave();
            gameState = GameConfig.GameState.InGame;
        });
    }
    public void StartStage(int stageID)
    {
        gameState = GameConfig.GameState.InGame_SpawningHero;
        stageCts = new CancellationTokenSource();
        timerCts = new CancellationTokenSource();
        UserData.Instance.AcquireSoul.Value = 0;
        //UserData.Instance.LocalData.Stamina.Value -= ConfigTable.Instance.StageStartCost;
         SetIngameUI();
        var waitTask = ingameUI.StartLoadingUI(); 
        if (currStageObj != null)
        {
            Destroy(currStageObj.gameObject);
        }
        var stageInfo = DataManager.Instance.GetStageInfoData(stageID);
        worldMap.gameObject.SetActive(false);
        UniTask.Create(async () =>
        {
            currStageOpHandler = Addressables.InstantiateAsync(stageInfo.prefabname, Vector3.zero, Quaternion.identity, objRoot);
            await currStageOpHandler;
            await waitTask;
            currStageObj = currStageOpHandler.Result.GetComponent<StageObject>();
            UserData.Instance.PlayingStage = stageID;
            ingameUI.EndLoadingUI();
            SetStageUI(timerCts);
            InitEnemies();
            InitInGameSpeed();
            InitFollowCamera().Forget();
            await SpawnAllHero();
            gameState = GameConfig.GameState.InGame;
        });
    }

    private async UniTaskVoid InitFollowCamera()
    {
        followCameraCts = new CancellationTokenSource();
        cameraManager.SetPosition(currStageObj.heroSpawnPos.position + currStageObj.FollowOffset);
        cameraManager.SetZoomAndSize(GameConfig.DefaultZoomSize, currStageObj.ZoomMin, currStageObj.ZoomMax, currStageObj.SizeMinX, currStageObj.SizeMaxX, currStageObj.SizeMinY, currStageObj.SizeMaxY);
        await UniTask.WaitUntil(() => heroUIDOrder.Count > 0, cancellationToken: followCameraCts.Token);
        cameraFollowTime = 2f;
        while (true)
        {
            if (cameraFollowTime >= 2f)
            {
                MHeroObj targetHero = GetFirstCameraTarget();
                if (targetHero != null)
                {
                    cameraManager.SetFollowObject(targetHero.gameObject, true, currStageObj.FollowOffset, null);
                    targetHero.SetDeadAction(() =>
                    {
                        cameraManager.CancelFollowTarget();
                    });
                }
                cameraFollowTime = 0f;
            }
            await UniTask.Yield(cancellationToken: followCameraCts.Token);
            cameraFollowTime += Time.deltaTime;
        }
    }
    private MHeroObj GetFirstCameraTarget()
    {
        foreach (var item in heroUIDOrder)
        {
            if (!UserData.Instance.isBattleHeroDead(item))
            {
                return GetHeroObj(item);
            }
        }
        return null;
    }
    private void InitInGameSpeed()
    {
        Time.timeScale = UserData.Instance.GameSpeed;
    }

    private void InitWorldGameSpeed()
    {
        Time.timeScale = 1f;
    }

    public void BackToWorld()
    {
        gameState = GameConfig.GameState.MainUI;
        SetWorldUI();
        cameraManager.CancelFollowTarget();
        worldMap.gameObject.SetActive(true);
        worldMap.UpdateWorld();
        cameraManager.SetZoomAndSize(GameConfig.DefaultZoomSize, 2, 7, -2, 9, -2, 6);
        FollowToCurrStage();
        InitWorldGameSpeed();
    }

    public void SetWorldUI()
    {
        mainUI.SetActive(true);
        ingameUI.SetActive(false);
    }
    public void SetIngameUI()
    {
        mainUI.SetActive(false);
        mainUI.HideStageInfo();
        ingameUI.SetActive(true);
        worldMap.SelectStage(-1);
    }
    public void SetStageUI(CancellationTokenSource _cts)
    {
        var stageInfo = DataManager.Instance.GetStageInfoData(UserData.Instance.PlayingStage);
        ingameUI.SetData(GameTime.Get() + stageInfo.stagecleartime, _cts);
    }
    public void FollowToCurrStage()
    {
        cameraManager.SetFollowObject(worldMap.GetCurrStageObj(), false, Vector2.zero, null);
    }

    private void DisposeCTS()
    {
        spawnHeroCts?.Cancel();
        stageCts?.Cancel();
        timerCts?.Cancel();
        followCameraCts?.Cancel();
    }
    private void SetAllUnitEndState()
    {
        foreach (var item in heroDic)
        {
            item.Value.SetEndState();
        }

        foreach (var item in enemyDic)
        {
            item.Value.SetEndState();
        }
    }

    public void RetryStage()
    {
        StartStage(UserData.Instance.PlayingStage);
    }

    public void NextStage()
    {
        int nextStage = UserData.Instance.PlayingStage + 1;
        var stageInfo = DataManager.Instance.GetStageInfoData(nextStage);
        if (stageInfo == null)
        {
            PopupManager.Instance.ShowSystemOneBtnPopup("There is no Stage", "OK");
            return;
        }
        StartStage(UserData.Instance.PlayingStage + 1);
    }

    public void RemoveStage()
    {
        if (!Addressables.ReleaseInstance(currStageOpHandler))
        {
            Destroy(currStageOpHandler.Result.gameObject);
        }
        RemoveAllBattleHero();
        RemoveAllEnemy();
        RemoveAllProjectile();
        //ResourceManagerTest.Instance.UnloadUnusedAssetsImmediate().Forget();
    }

    private void InitGame()
    {
        gameState = GameConfig.GameState.MainUI;
        cameraManager.SetTouchAction(() =>
        {
            if (gameState == GameConfig.GameState.MainUI)
            {
                GameObject obj = cameraManager.TryGetRayCastObject(Input.mousePosition, GameConfig.StageSlotLayerMask);
                if (obj != null)
                {
                    cameraManager.SetFollowObject(obj, false, Vector2.zero, () =>
                    {
                        WorldMapStageSlot stageSlot = obj.GetComponent<WorldMapStageSlot>();
                        mainUI.ShowStageInfo(stageSlot.stage, () =>
                        {
                            // startBtn
                            StartStage(stageSlot.stage);
                        });
                        worldMap.SelectStage(stageSlot.stage);

                        CheckStageGold(stageSlot.stage, stageSlot.transform.position);
                    });
                }
                else
                {
                    cameraManager.CancelFollowTarget();
                    mainUI.HideStageInfo();
                    worldMap.SelectStage(-1);
                }
            }
            else
            {
                cameraManager.CancelFollowTarget();
                cameraFollowTime = 0f;
            }
        }, 
        ()=> {
            if (gameState == GameConfig.GameState.MainUI)
            {
                mainUI.HideStageInfo();
            }
            else
            {
                cameraManager.CancelFollowTarget();
                cameraFollowTime = 0f;
            }
        });
        worldMap.InitWorld();
        mainUI.InitTabGroup();
        SetWorldUI();
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
            UnitBattleData data = UserData.Instance.AddEnemyData(enemyObj.TID);
            enemyObj.InitObject(data.battleUID, true, (_attackData) => {

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
            enemyDic.Add(data.battleUID, enemyObj);
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
                    //if (heroObj.State == MHeroObj.FSMStates.Idle || heroObj.State == MHeroObj.FSMStates.WaypointMove || heroObj.State == MHeroObj.FSMStates.DashMove)
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
        if (gameState == GameConfig.GameState.BossDefeatEffect || gameState == GameConfig.GameState.GameEnd)
            return;

        UserData.Instance.KillBattleHero(_attackerUID, _uid);
        if (gameState == GameConfig.GameState.InGame && UserData.Instance.isAllHeroDead())
        {
            LoseStage();
        }

        if (heroDic.ContainsKey(_uid))
        {
            MHeroObj heroObj = heroDic[_uid];
            heroObj.GetKilled();
            Lean.Pool.LeanPool.Despawn(heroObj.gameObject);
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
        if (gameState == GameConfig.GameState.BossDefeatEffect || gameState == GameConfig.GameState.GameEnd)
            return;

        UserData.Instance.KillEnemy(attackerUID, _uid);
        if (enemyDic.ContainsKey(_uid))
        {
            var enemyObj = enemyDic[_uid];
            var effect = MResourceManager.Instance.GetBuildResource(enemyObj.UnitData.refData.deatheffect).GetComponent<EffectFeedback>();
            var effectPeedback = Lean.Pool.LeanPool.Spawn(effect, enemyObj.transform.position, Quaternion.identity, objRoot);
            if (_uid == enemyBossUID)
            {
                gameState = GameConfig.GameState.BossDefeatEffect;
                DisposeCTS();
                Time.timeScale = 0.3f;
                cameraManager.EnableCameraControl = false;
                cameraManager.SetFollowObject(enemyObj.gameObject, false, Vector2.zero, null);
                SetAllUnitEndState();
                effectPeedback.SetData(() =>
                { 
                    Lean.Pool.LeanPool.Despawn(effectPeedback);
                    Time.timeScale = 1f;
                    WinStage();
                });
                effectPeedback.SetTimeScaleAction(_scale =>
                {
                    Time.timeScale = _scale;
                });
            }
            else
            {
                effectPeedback.SetData(() =>
                {
                    Lean.Pool.LeanPool.Despawn(effectPeedback);
                });

                var soulObj = Lean.Pool.LeanPool.Spawn(soulRewardPrefab, enemyObj.transform.position, Quaternion.identity, objRoot);
                soulObj.Shoot(ingameUI.SoulTarget, () => {
                    UserData.Instance.AcquireSoul.Value += enemyObj.UnitData.refUnitGradeData.dropsoulcnt;
                });
            }
            
            enemyObj.GetKilled();
            enemyObj.gameObject.SetActive(false);
        }
    }

    private void RemoveEnemy(int _uid)
    {
        UserData.Instance.RemoveEnmey(_uid);
        if (enemyDic.ContainsKey(_uid))
        {
            //Destroy(enemyDic[_uid].gameObject);
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

    private void SpawnBattleEnemy()
    {
        var randNum = Random.Range(0, currStageObj.enemySpawnPos.Count);
        
        UnitBattleData data = UserData.Instance.AddEnemyData(1001);
        Vector3 spawnPos = currStageObj.enemySpawnPos[randNum].position + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0);

        GameObject unitPrefab = MResourceManager.Instance.GetPrefab(data.refData.prefabname);
        MEnemyObj enemyObj = Lean.Pool.LeanPool.Spawn(unitPrefab, spawnPos, Quaternion.identity, objRoot).GetComponent<MEnemyObj>();

        enemyObj.InitObject(data.battleUID, true, (_attackData) =>
        {
            var heroObj = GetHeroObj(_attackData.attackerUID);
            if (heroObj == null)
            {
                // To Do : ??
                return;
            }
            DoEnemyGetDamage(enemyObj, heroObj.transform.position, _attackData.attackerUID, _attackData.damage);
        });
        enemyDic.Add(data.battleUID, enemyObj);
    }
    private void SpawnBattleHero(int _uid)
    {
        var heroData = UserData.Instance.GetHeroData(_uid);
        var battleHeroData = UserData.Instance.AddBattleHeroData(heroData);
        Vector3 spawnPos = currStageObj.heroSpawnPos.position + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0);

        GameObject unitPrefab = MResourceManager.Instance.GetPrefab(battleHeroData.refData.prefabname);
        MHeroObj heroObj = Lean.Pool.LeanPool.Spawn(unitPrefab, spawnPos, Quaternion.identity, objRoot).GetComponent<MHeroObj>();
        heroObj.InitObject(battleHeroData.battleUID, false, (_attackData) =>
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
        heroDic.Add(battleHeroData.battleUID, heroObj);
        heroUIDOrder.Add(battleHeroData.battleUID);
    }

    private async UniTask SpawnAllHero()
    {
        heroUIDOrder = new List<int>();
        heroDic = new Dictionary<int, MHeroObj>();
        spawnHeroCts?.Cancel();
        spawnHeroCts = new CancellationTokenSource();
        await UniTask.Delay(1000, cancellationToken: spawnHeroCts.Token);
        foreach (var item in UserData.Instance.LocalData.BattlePartyDic)
        {
            if (item.Value != -1)
            {
                var heroData = UserData.Instance.GetHeroData(item.Value);
                for (int i = 0; i < heroData.refUnitGradeData.summoncnt; i++)
                {
                    SpawnBattleHero(item.Value);
                    await UniTask.Delay(300, cancellationToken: spawnHeroCts.Token);
                }
            }
        }
    }

    private async UniTask StartEnemyWave()
    {
        if (currStageObj.enemySpawnPos.Count > 0)
        {
            while (true)
            {
                await UniTask.WaitForSeconds(0.5f);
                SpawnBattleEnemy();
            }
        }    
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
        followCameraCts?.Cancel();
        followCameraCts?.Dispose();
        timerCts?.Cancel();
        timerCts?.Dispose();
    }
}
