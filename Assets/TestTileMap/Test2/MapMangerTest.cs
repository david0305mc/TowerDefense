using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class MapMangerTest : SingletonMono<MapMangerTest>
{

    [SerializeField] private TestHeroObj testHeroObj;
    [SerializeField] public Tilemap tileMap;
    [SerializeField] private TileBase tilebase;
    //TileBase
    
    private Vector3 heroPos;

    private void Start()
    {
        TestGroundManager.Instance.UpdateAllNodes();

        testHeroObj.StartFSM();
    }
    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            heroPos = testHeroObj.transform.position;
            heroPos = new Vector3(heroPos.x, heroPos.y, worldPos.z);

            var heroGridPos = tileMap.WorldToCell(heroPos);
            var targetGridPos = tileMap.WorldToCell(worldPos);
            var targetWorldPos = tileMap.GetCellCenterWorld(targetGridPos);

            Debug.Log($"Start gridpos {heroGridPos}");
            Debug.Log($"End gridpos {targetGridPos}");

            var path = TestGroundManager.Instance.GetPath(new Vector3(heroGridPos.x, heroGridPos.z, heroGridPos.y), new Vector3(targetGridPos.x, targetGridPos.z, targetGridPos.y) , false);

            //foreach (var item in path.nodes)
            //{
            //    Debug.Log($"pathNode  {item}");
            //}
            
            var targetWorldPos2 = tileMap.GetCellCenterWorld(new Vector3Int((int)path.nodes.Last().x, (int)path.nodes.Last().z, (int)path.nodes.Last().y));

            testHeroObj.MoveTo(path);

            //testHeroObj.MoveTo(new Vector3(targetWorldPos2.x, targetWorldPos2.y, 2));
        }

        //if (Input.GetMouseButtonUp(0))
        //{
        //    var gridPos = tileMap.WorldToCell(prevPos);
        //    var cellWorldPos = tileMap.GetCellCenterWorld(gridPos);
        //    Debug.Log($"gridpos {gridPos}");
        //    testHeroObj.MoveTo(new Vector3(cellWorldPos.x, cellWorldPos.y, 2));
        //}


    }
}
