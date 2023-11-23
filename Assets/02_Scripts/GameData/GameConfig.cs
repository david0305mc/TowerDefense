using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game
{
    public static class GameConfig
    {
        public enum GameState
        { 
            MainUI,
            InGame_SpawningHero,
            InGame,
            BossDefeatEffect,
            GameEnd,
        }

        public static readonly float WorldMapDefaultZoomSize = 3;
        public static readonly float WorldMapZoomMin = 2;
        public static readonly float WorldMapZoomMax = 5;
        public static readonly int WorldMapSizeMinX = -12;
        public static readonly int WorldMapSizeMaxX = 12;
        public static readonly int WorldMapSizeMinY = -4;
        public static readonly int WorldMapSizeMaxY = 6;

        public static readonly int OfflineRewardMinSecond = 60;
        public static readonly int MaxBattlePartyCount = 6;
        public static readonly int ItemSelectedLayerOrder = 10;
        public static readonly float projectileRewardSizeFactor = 10f;
        public static readonly float unitTargetDragSpeed = 0.3f;
        public static readonly float normalTargetDragSpeed = 1f;

        public static readonly int CanvasMainUILayerOrder = 100;
        public static int CanvasPopupManagerLayerOrder = 300;
        public static readonly int StartBuildingID = 10000;

        public static readonly int WaveStageID_01 = 999;
        public static readonly int WaveStageID_02 = 1000;
        public static readonly int DevilCasleID = 20001;
        public static readonly int UserObjectUID = 10000;

        public static readonly int LastTutorial = 19;

        public static readonly string DefaultLayerName = "Default";
        public static readonly string ForegroundLayerName = "Foreground";
        public static readonly string UILayerName = "UI";
        public static int UnitLayerMask = LayerMask.GetMask("Unit");
        public static int ItemLayerMask = LayerMask.GetMask("Item");
        public static int GroundLayerMask = LayerMask.GetMask("Ground");
        public static int StageSlotLayerMask = LayerMask.GetMask("StageSlot");
        public static int ShopRewardLayerMask = LayerMask.GetMask("ShipReward");
        public static int UILayerMask = LayerMask.GetMask("UI");

        public static readonly string BoxIcon01 = "Images/Round/hide_box_img_1";

        public static Dictionary<float, GameType.Direction> AngleToDirectionMap = new Dictionary<float, GameType.Direction>() {
        {1, GameType.Direction.BOTTOM_RIGHT },
        {51, GameType.Direction.BOTTOM },
        {110, GameType.Direction.BOTTOM_LEFT },
        {153, GameType.Direction.LEFT },
        {190, GameType.Direction.TOP_LEFT },
        {220, GameType.Direction.TOP },
        {290, GameType.Direction.TOP_RIGHT },
        {357, GameType.Direction.RIGHT } };


        public static Vector3 PositiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    }

    public class GamePath
    {
        public static readonly string PopupPath = "Popup";

        public static string GetImagePath(string path)
        {
            return Path.Combine("Images", path);
        }

    }


    public enum ObjStatus
    {
        Idle,
        Walk,
        Attack,
    }


    public enum RenderingLayer
    {
        GROUND = 0,
        SHADOW = 1,
        SPRITE = 2
    }

    public enum StageStatus
    { 
        Normal,
        Occupation,
        Lock,
    }
}
