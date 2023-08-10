using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMangerTest : SingletonMono<MapMangerTest>
{

    [SerializeField] private TestHeroObj testHeroObj;
    [SerializeField] public Tilemap tileMap;
    [SerializeField] private TileBase tilebase;
    //TileBase
    
    private Vector3 prevPos;

    private void Start()
    {
        TestGroundManager.Instance.UpdateAllNodes();


    }
    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            
            prevPos = testHeroObj.transform.position;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var heroGridPos = tileMap.WorldToCell(prevPos);
            var targetGridPos = tileMap.WorldToCell(worldPos);
            var targetWorldPos = tileMap.GetCellCenterWorld(targetGridPos);

            var path = TestGroundManager.Instance.GetPath(new Vector3(heroGridPos.x, 0, heroGridPos.y), new Vector3(targetGridPos.x, 0, targetGridPos.y) , false);

            foreach (var item in path.nodes)
            {
                Debug.Log($"pathNode  {item}");
            }
            testHeroObj.MoveTo(path);
            //testHeroObj.MoveTo(new Vector3(targetWorldPos.x, targetWorldPos.y, 2));
            Debug.Log($"gridpos {targetGridPos}");
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
