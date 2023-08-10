using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMangerTest : MonoBehaviour
{

    [SerializeField] private GameObject TestObj;
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private TileBase tilebase;
    //TileBase
    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var gridPos = tileMap.WorldToCell(worldPos);
            
            var cellWorldPos = tileMap.GetCellCenterWorld(gridPos);
            TestObj.transform.position = new Vector3(cellWorldPos.x, cellWorldPos.y, 2);
        }
        
        
    }
}
