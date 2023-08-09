using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MOverlayTile : MonoBehaviour
{
    public int G;
    public int H;

    public int F { get { return G + H; } }
    public bool isBlocked;
    public MOverlayTile previous;
    public Vector3Int gridLocation;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HideTile();  
        }
    }
    public void ShowTile()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
    }

    public void HideTile()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
    }
}
