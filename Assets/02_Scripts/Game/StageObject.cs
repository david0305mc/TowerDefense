using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StageObject : MonoBehaviour
{
    public Transform enemyObjRoot;
    public Transform heroSpawnPos;
    public List<Transform> enemySpawnPos;
    public Tilemap tileMap;
    public Tilemap obstacleTileMap;
    public Tilemap treeTileMap;
    public List<GameObject> wayPointLists;
    public Transform devileCastleSpawnPoint;

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
        Gizmos.color = Color.red;
        if (wayPointLists.Count > 0)
        {
            for (int i = 0; i < wayPointLists.Count - 1; i++)
            {
                if (wayPointLists[i] == null || wayPointLists[i + 1] == null)
                    continue;
                Gizmos.DrawLine(wayPointLists[i].transform.position, wayPointLists[i + 1].transform.position);
            }
        }

        {
            Gizmos.color = new Color(1.0f, 0, 0, 1f);
            Gizmos.matrix = transform.worldToLocalMatrix;

            Vector3 topLeft = new Vector3(SizeMinX, SizeMaxY);
            Vector3 topRight = new Vector3(SizeMaxX, SizeMaxY);
            Vector3 botRight = new Vector3(SizeMaxX, SizeMinY);
            Vector3 botLeft = new Vector3(SizeMinX, SizeMinY);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, botRight);
            Gizmos.DrawLine(botRight, botLeft);
            Gizmos.DrawLine(botLeft, topLeft);
        }

    }
}

