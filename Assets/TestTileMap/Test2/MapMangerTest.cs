using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class MapMangerTest : SingletonMono<MapMangerTest>
{

    [SerializeField] private ProjectileStraight projStraight;
    [SerializeField] private ProjectileParabola projParabola;

    [SerializeField] private MHeroObj testHeroObj;
    [SerializeField] public Tilemap tileMap;
    [SerializeField] private TileBase tilebase;

    [SerializeField] private Transform objRoot;
    //TileBase

    private Vector3 heroPos;
    private List<MEnemyObj> enemyLists;

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();
        var enemies = objRoot.GetComponentsInChildren<MEnemyObj>();
        enemyLists = enemies.ToList();
    }

    public MEnemyObj GetNearestEnemyObj(Vector3 srcPos)
    {
        MEnemyObj nearestObj = null;
        float shortDist = float.MaxValue;
        foreach (var item in enemyLists)
        {
            float dist = Vector3.Distance(srcPos, item.transform.position);
            
            if (dist < shortDist)
            {
                shortDist = dist;
                nearestObj = item;
            }
        }

        return nearestObj;
    }
    private void Start()
    {
        TestGroundManager.Instance.UpdateAllNodes();

        testHeroObj.StartFSM();
    }

    public void LauchProjectile(MHeroObj heroObj, MEnemyObj enemyObj)
    {
        ProjectileStraight bullet = Lean.Pool.LeanPool.Spawn(projStraight, heroObj.transform.position, Quaternion.identity, objRoot);
        bullet.Shoot(enemyObj, 1);

    }

    public void RemoveEnemy(MEnemyObj targetObj)
    {
        Destroy(targetObj.gameObject);
        enemyLists.Remove(targetObj);
    }
    public TestGroundManager.Path GetPath(Vector3 heroPos, Vector3 targetPos)
    {    //        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    
        heroPos = new Vector3(heroPos.x, heroPos.y, -10);
        targetPos = new Vector3(targetPos.x, targetPos.y, -10);
        var heroGridPos = tileMap.WorldToCell(heroPos);
        var targetGridPos = tileMap.WorldToCell(targetPos);
        var targetWorldPos = tileMap.GetCellCenterWorld(targetGridPos);
        return TestGroundManager.Instance.GetPath(new Vector3(heroGridPos.x, heroGridPos.z, heroGridPos.y), new Vector3(targetGridPos.x, targetGridPos.z, targetGridPos.y), false);
    }
    //private void Update()
    //{

    //    if (Input.GetMouseButtonDown(0))
    //    {

    //        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //        heroPos = testHeroObj.transform.position;
    //        heroPos = new Vector3(heroPos.x, heroPos.y, worldPos.z);

    //        var heroGridPos = tileMap.WorldToCell(heroPos);
    //        var targetGridPos = tileMap.WorldToCell(worldPos);
    //        var targetWorldPos = tileMap.GetCellCenterWorld(targetGridPos);

    //        Debug.Log($"Start gridpos {heroGridPos}");
    //        Debug.Log($"End gridpos {targetGridPos}");

    //        var path = TestGroundManager.Instance.GetPath(new Vector3(heroGridPos.x, heroGridPos.z, heroGridPos.y), new Vector3(targetGridPos.x, targetGridPos.z, targetGridPos.y) , false);

    //        var targetWorldPos2 = tileMap.GetCellCenterWorld(new Vector3Int((int)path.nodes.Last().x, (int)path.nodes.Last().z, (int)path.nodes.Last().y));

    //        testHeroObj.MoveTo(path);

    //        //testHeroObj.MoveTo(new Vector3(targetWorldPos2.x, targetWorldPos2.y, 2));
    //    }

    //}
}
