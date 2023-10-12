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

    public Vector3 FollowOffset = new Vector3(2, 3, 0);
    public float DefaultZoomSize = 5;
    public float ZoomMin = 2;
    public float ZoomMax = 20;
    public int SizeMinX = -10;
    public int SizeMaxX = 25;
    public int SizeMinY = -10;
    public int SizeMaxY = 25;

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

