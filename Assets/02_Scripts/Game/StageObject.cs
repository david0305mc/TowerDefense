using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StageObject : MonoBehaviour
{
    public Transform enemyObjRoot;
    public Transform heroSpawnPos;
    public Tilemap tileMap;
    public Tilemap obstacleTileMap;
    public Tilemap treeTileMap;
    public List<GameObject> wayPointLists;
    

    private void OnDrawGizmos()
    {
        if (wayPointLists.Count <= 1)
            return;
        Gizmos.color = Color.red;
        for (int i = 0; i < wayPointLists.Count - 1; i++)
        {
            if (wayPointLists[i] == null || wayPointLists[i + 1] == null)
                continue;
            Gizmos.DrawLine(wayPointLists[i].transform.position, wayPointLists[i + 1].transform.position);
        }
    }
}

