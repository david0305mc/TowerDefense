using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MPathFinder 
{
    public List<MOverlayTile> FindPath(MOverlayTile start, MOverlayTile end)
    {
        List<MOverlayTile> openList = new List<MOverlayTile>();
        List<MOverlayTile> closedList = new List<MOverlayTile>();

        openList.Add(start);

        while (openList.Count > 0)
        {
            MOverlayTile currentOverlayTile = openList.OrderBy(x => x.F).First();
            openList.Remove(currentOverlayTile);
            closedList.Add(currentOverlayTile);

            if (currentOverlayTile == end)
            {
                // final
                return GetFinishedList(start, end);
            }

            var neighbourTiles = GetNeighbourTiles(currentOverlayTile);
            foreach (var neighbour in neighbourTiles)
            {
                if (neighbour.isBlocked || closedList.Contains(neighbour) || Mathf.Abs(currentOverlayTile.gridLocation.z - neighbour.gridLocation.z) > 1)
                {
                    continue;
                }
                neighbour.G = GetManhanttenDistance(start, neighbour);
                neighbour.H = GetManhanttenDistance(end, neighbour);
                neighbour.previous = currentOverlayTile;

                if (!openList.Contains(neighbour))
                {
                    openList.Add(neighbour);
                }
            }
        }
        return new List<MOverlayTile>();
    }

    private List<MOverlayTile> GetFinishedList(MOverlayTile start, MOverlayTile end)
    {
        List<MOverlayTile> finishedList = new List<MOverlayTile>();
        MOverlayTile currentTile = end;

        while (currentTile != start)
        {
            finishedList.Add(currentTile);
            currentTile = currentTile.previous;
        }

        finishedList.Reverse();
        return finishedList;
    }

    private int GetManhanttenDistance(MOverlayTile start, MOverlayTile neighbour)
    {
        return Mathf.Abs(start.gridLocation.x - neighbour.gridLocation.x) + Mathf.Abs(start.gridLocation.y - neighbour.gridLocation.y);
    }
    private List<MOverlayTile> GetNeighbourTiles(MOverlayTile currOverlaytile)
    {
        var map = MMapManager.Instance.map;
        List<MOverlayTile> neighbours = new List<MOverlayTile>();

        // top
        Vector2Int localtionToCheck = new Vector2Int(currOverlaytile.gridLocation.x, currOverlaytile.gridLocation.y + 1);
        if (map.ContainsKey(localtionToCheck))
        {
            neighbours.Add(map[localtionToCheck]);
        }

        // right
        localtionToCheck = new Vector2Int(currOverlaytile.gridLocation.x + 1, currOverlaytile.gridLocation.y);
        if (map.ContainsKey(localtionToCheck))
        {
            neighbours.Add(map[localtionToCheck]);
        }

        // left
        localtionToCheck = new Vector2Int(currOverlaytile.gridLocation.x - 1, currOverlaytile.gridLocation.y);
        if (map.ContainsKey(localtionToCheck))
        {
            neighbours.Add(map[localtionToCheck]);
        }

        // bottom
        localtionToCheck = new Vector2Int(currOverlaytile.gridLocation.x, currOverlaytile.gridLocation.y - 1);
        if (map.ContainsKey(localtionToCheck))
        {
            neighbours.Add(map[localtionToCheck]);
        }

        return neighbours;
    }


}
