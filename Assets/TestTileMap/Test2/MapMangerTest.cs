using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMangerTest : MonoBehaviour
{

    [SerializeField] private TestHeroObj testHeroObj;
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private TileBase tilebase;
    //TileBase

    private Vector3 prevPos;
    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            prevPos = testHeroObj.transform.position;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var gridPos = tileMap.WorldToCell(worldPos);
            var cellWorldPos = tileMap.GetCellCenterWorld(gridPos);

            testHeroObj.MoveTo(new Vector3(cellWorldPos.x, cellWorldPos.y, 2));
            Debug.Log($"gridpos {gridPos}");
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
