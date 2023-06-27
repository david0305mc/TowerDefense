using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUtil 
{
    public static Vector2 WorldPositionToScreenSpaceCameraPosition(Camera worldCamera, Canvas canvas, Vector3 position)
    {
        Vector2 viewport = worldCamera.WorldToViewportPoint(position);
        Ray canvasRay = canvas.worldCamera.ViewportPointToRay(viewport);
        return canvasRay.GetPoint(canvas.planeDistance);
    }
}

