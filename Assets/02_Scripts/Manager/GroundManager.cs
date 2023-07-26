using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class GroundManager : Singleton<GroundManager>
{
    public class Path
    {
        public Vector3[] nodes;

        public float GetDistanceAlongPath()
        {
            float distance = 0;
            if (nodes == null || nodes.Length <= 1)
            {
                return 0;
            }
            else
            {
                for (int index = 0; index < nodes.Length - 1; index++)
                {
                    distance += Vector3.Distance(nodes[index], nodes[index + 1]);
                }
                return distance;
            }
        }
    }
    public enum Action
    {
        ADD,
        REMOVE
    }

    public const int nodeWidth = 44;
    public const int nodeHeight = 44;

    private int[,] instanceNodes;
    private bool[,] pathNodesWithoutWall;
    private bool[,] pathNodesWithWall;


    public Path GetPath(Vector3 startPoint, Vector3 endPoint, bool considerWalls)
    {
        Path path = new Path();

        if (endPoint.x < 0 || endPoint.x >= nodeWidth || endPoint.z < 0 || endPoint.z >= nodeHeight)
        {
            Debug.LogError("The target point is out of the grid!");
            return path;
        }
        Vector2 startPointInMap = new Vector2(startPoint.x, startPoint.z);
        Vector2 endPointInMap = new Vector2(endPoint.x, endPoint.z);
        SearchParameters searchParameter = null;
        if (considerWalls)
        {
            searchParameter = new SearchParameters(startPointInMap, endPointInMap, pathNodesWithWall);
        }
        else
        {
            searchParameter = new SearchParameters(startPointInMap, endPointInMap, pathNodesWithoutWall);
        }
        PathFinder pathFinder = new PathFinder(searchParameter);
        List<Vector2> points = pathFinder.FindPath();

        int index = -1;
        List<Vector3> nodes = new List<Vector3>();
        foreach (Vector2 point in points)
        {
            index++;
            Vector3 pointInGround = new Vector3(point.x, 0, point.y);
            nodes.Add(pointInGround);
        }
        path.nodes = nodes.ToArray();
        return path;
    }

    public void UpdateAllNodes()
    {
        this.instanceNodes = new int[nodeWidth, nodeHeight];
        this.pathNodesWithoutWall = new bool[nodeWidth, nodeHeight];
        this.pathNodesWithWall = new bool[nodeWidth, nodeHeight];

        for (int x = 0; x < nodeWidth; x++)
        {
            for (int z = 0; z < nodeHeight; z++)
            {
                this.instanceNodes[x, z] = -1;
                //this.pathNodesWithoutWall[x, z] = Random.Range(0, 2) == 0 ? true : false;
                this.pathNodesWithoutWall[x, z] = true;
                this.pathNodesWithWall[x, z] = true;
            }
        }

        foreach (KeyValuePair<int, BaseObj> entry in GameManager.Instance.GetBaseObjDic)
        {
            BaseObj item = entry.Value;
            //if (!item.itemData.configuration.isCharacter)
            {
                this.UpdateBaseItemNodes(item, Action.ADD);
            }
        }
    }

    public void UpdateBaseItemNodes(BaseObj item, Action action)
    {
        Vector3 pos = item.transform.localPosition;

        int x = (int)(pos.x);
        int z = (int)(pos.z);
        //int sizeX = (int)item.GetSize().x;
        //int sizeZ = (int)item.GetSize().z;
        int sizeX = 1;
        int sizeZ = 1;

        for (int indexX = x; indexX < x + sizeX; indexX++)
        {
            for (int indexZ = z; indexZ < z + sizeZ; indexZ++)
            {
                bool isCellWalkable = false;
                if ((sizeX > 2 && indexX == x) || (sizeX > 2 && indexX == x + sizeX - 1) || (sizeZ > 2 && indexZ == z) || (sizeZ > 2 && indexZ == z + sizeZ - 1))
                {
                    //use this for make outer edge walkable for items have size morethan 2x2
                    isCellWalkable = true;
                }

                this.pathNodesWithoutWall[indexX, indexZ] = isCellWalkable;
                //if (item.itemData.name == "ArmyCamp")
                //{
                //    //make every cell walkable in army camp
                //    isCellWalkable = true;
                //}

                //if (action == Action.ADD)
                //{
                //    //adding scene item to nodes, so walkable is false
                //    this.instanceNodes[indexX, indexZ] = item.instanceId;

                //    if (item.itemData.name == "Wall")
                //    {
                //        this.pathNodesWithoutWall[indexX, indexZ] = true;
                //        this.pathNodesWithWall[indexX, indexZ] = false;
                //    }
                //    else
                //    {
                //        this.pathNodesWithoutWall[indexX, indexZ] = isCellWalkable;
                //    }

                //}
                //else if (action == Action.REMOVE)
                //{
                //    if (this.instanceNodes[indexX, indexZ] == item.instanceId)
                //    {
                //        this.instanceNodes[indexX, indexZ] = -1;
                //        this.pathNodesWithoutWall[indexX, indexZ] = true;
                //        this.pathNodesWithWall[indexX, indexZ] = true;
                //    }
                //}

            }
        }
    }
    public Vector3 GetNearestOutCell(Vector3 targetPosition, int size)
    {
        var outCells = GetOutCells(targetPosition, size);
        float nearestDist = 9999f;
        Vector3 _currentTargetPoint = targetPosition;
        foreach (var item in outCells)
        {
            var dist = Vector3.Distance(item, targetPosition);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                _currentTargetPoint = item;
            }
        }
        return _currentTargetPoint;
    }
    public Vector3[] GetOutCells(Vector3 targetPosition, int size)
    {
        List<Vector3> cells = new List<Vector3>();
        Enumerable.Range(0, size).ToList().ForEach(i => {
            Enumerable.Range(0, size).ToList().ForEach(j => {
                if (i == 0 || j == 0 || i == size || j == size)
                {
                    Vector3 pos = targetPosition + new Vector3(i, 0, -j);
                    if (cells.Contains(pos))
                    {
                        cells.Add(pos);
                    }
                }
            });
        });
        return cells.ToArray();
    }

    public bool IsInNodeRange(Vector3 point)
    {
        if (point.x < 0 || point.z < 0)
            return false;

        if (point.x >= nodeWidth || point.z >= nodeHeight)
            return false;
        return true;

    }
    public Vector3 GetRandomFreePosition()
    {
        int x = Random.Range(5, nodeWidth - 5);
        int z = Random.Range(5, nodeHeight - 5);

        if (this.instanceNodes[x, z] != -1)
        {
            return GetRandomFreePosition();
        }
        return new Vector3(x, 0, z);
    }

    public Vector3 GetRandomFreePositionForItem()
    {
        Vector3 randomPosition = new Vector3(Random.Range(0, nodeWidth), 0, Random.Range(0, nodeHeight));
        return randomPosition;
    }
}

