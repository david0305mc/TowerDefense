using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GameConfig
{
    public static readonly int ItemSelectedLayerOrder = 10;

    public static int ItemLayerMask = LayerMask.GetMask("Item");
    public static int GroundLayerMask = LayerMask.GetMask("Ground");

    public static readonly string BoxIcon01 = "Images/Round/hide_box_img_1";

}

public class GamePath
{
    public static readonly string PopupPath = "Popup";

    public static string GetImagePath(string path)
    {
        return Path.Combine("Images", path);
    }

}


public enum SchoolZone
{
    SchoolZone_A,
    SchoolZone_B,
    SchoolZone_C,
}