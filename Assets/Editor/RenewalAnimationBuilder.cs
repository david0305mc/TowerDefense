using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Builds editable, articulated cutout rigs and native Unity animation assets.</summary>
public static class RenewalAnimationBuilder
{
    private const string Art = "Assets/04_Images/Renewal/Parts/RightFacing/";
    private const string Output = "Assets/07_Animations/Character/Renewal";
    private const string Prefabs = "Assets/03_Prefabs/InGame/Character/Renewal/";
    private const string Upper = "Visual/Hips/Upper";
    private const string FrontArm = Upper + "/FrontArm";
    private const string FrontHand = FrontArm + "/Forearm";
    private const string Weapon = FrontHand + "/Weapon";
    private const string BackArm = Upper + "/BackArm";
    private const string BackHand = BackArm + "/Forearm";
    private const float ThighLength = .16f;
    private const float ShinLength = .198f;
    public const float PreviewFootFloor = 0;
    public static float WalkDuration(int character) => character == 0 ? .90f : character == 1 ? .98f : 1.04f;
    public static float PreviewWalkSpeed(int character) => .18f / (.62f * WalkDuration(character));
    private static readonly string[] Names = { "Mushroom_Spearguard", "Lantern_Mage", "Acorn_Shieldguard" };
    private static readonly string[] Motions = { "Idle", "Walk", "Attack", "AttackIdle", "Hurt", "Death" };
    private static readonly string[] Parts = { "Head", "Body", "BackArm", "BackHand", "FrontArm", "FrontHand", "BackThigh", "BackShin", "FrontThigh", "FrontShin", "Weapon", "Accessory", "Trail", "Impact", "Projectile", "Dust" };

    [MenuItem("Tools/Renewal Characters/Open preview")]
    private static void OpenPreview()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene("Assets/01_Scenes/RenewalCharacterPreview.unity");
    }

    [MenuItem("Tools/Renewal Characters/Rebuild animations")]
    public static void Build()
    {
        Directory.CreateDirectory(Output);
        Directory.CreateDirectory(Prefabs);
        AssetDatabase.Refresh();
        for (int i = 0; i < Names.Length; i++)
        {
            var sprites = ImportParts(Names[i]);
            string folder = Output + "/" + Names[i];
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            string controllerPath = folder + "/" + Names[i] + ".controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine = controller.layers[0].stateMachine;
            foreach (var child in machine.states) machine.RemoveState(child.state);
            GameObject root = MakeRig(i, sprites);
            try
            {
                for (int j = 0; j < Motions.Length; j++)
                {
                    var generated = MakeClip(i, Motions[j], root);
                    string path = folder + "/" + Motions[j] + ".anim";
                    var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if (clip == null) { clip = generated; AssetDatabase.CreateAsset(clip, path); }
                    else { EditorUtility.CopySerialized(generated, clip); UnityEngine.Object.DestroyImmediate(generated); EditorUtility.SetDirty(clip); }
                    var state = machine.AddState(Motions[j], new Vector3(250, 60 + j * 65, 0));
                    state.motion = clip;
                    state.writeDefaultValues = false;
                    if (j == 0) machine.defaultState = state;
                }
                root.GetComponent<Animator>().runtimeAnimatorController = controller;
                PrefabUtility.SaveAsPrefabAsset(root, Prefabs + Names[i] + ".prefab");
                EditorUtility.SetDirty(controller);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Validate();
    }

    private static Dictionary<string, Sprite> ImportParts(string name)
    {
        string path = Art + name + "_Parts.png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing parts atlas: " + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.isReadable = true;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.spritePixelsPerUnit = 100;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var pixels = texture.GetPixels32();
        var regions = AtlasRegions(pixels, texture.width, texture.height);
        var sheet = new SpriteMetaData[16];
        for (int n = 0; n < 16; n++)
        {
            var assigned = regions.Where(r => r.Cell == n && r.Count >= 20).OrderByDescending(r => r.Count).ToArray();
            if (assigned.Length == 0) throw new InvalidOperationException("Empty atlas part: " + name + "/" + Parts[n]);
            // Body parts are single silhouettes; VFX may consist of several disconnected wisps or sparks.
            var selected = n < 12 ? assigned.Take(1).ToArray() : assigned;
            int minX = Mathf.Max(0, selected.Min(r => r.MinX) - 2), minY = Mathf.Max(0, selected.Min(r => r.MinY) - 2);
            int maxX = Mathf.Min(texture.width - 1, selected.Max(r => r.MaxX) + 2), maxY = Mathf.Min(texture.height - 1, selected.Max(r => r.MaxY) + 2);
            Vector2 pivot = new Vector2(.5f, .5f);
            if (n == 0) pivot.y = .12f;
            if (n >= 2 && n <= 9) pivot.y = .90f;
            if (n == 7 || n == 9) pivot = new Vector2(.35f, 0);
            if (n == 10) pivot.y = .25f;
            if (n == 11 && name == Names[1]) pivot.y = .92f;
            sheet[n] = new SpriteMetaData { name = Parts[n], rect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1), alignment = (int)SpriteAlignment.Custom, pivot = pivot };
        }
        importer.spritesheet = sheet;
        importer.isReadable = false;
        importer.SaveAndReimport();
        var sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s => s.name);
        if (sprites.Count != 16) throw new InvalidOperationException("Expected 16 sprites: " + name);
        return sprites;
    }

    private sealed class AtlasRegion
    {
        public int Cell, Count, MinX, MinY, MaxX, MaxY;
    }

    private static List<AtlasRegion> AtlasRegions(Color32[] pixels, int width, int height)
    {
        var result = new List<AtlasRegion>(); var visited = new bool[pixels.Length];
        var queue = new Queue<int>();
        for (int seed = 0; seed < pixels.Length; seed++)
        {
            if (visited[seed] || pixels[seed].a < 96) continue;
            var region = new AtlasRegion { MinX = width, MinY = height }; var votes = new int[16];
            visited[seed] = true; queue.Enqueue(seed);
            while (queue.Count > 0)
            {
                int index = queue.Dequeue(), x = index % width, y = index / width;
                region.Count++; region.MinX = Mathf.Min(region.MinX, x); region.MaxX = Mathf.Max(region.MaxX, x);
                region.MinY = Mathf.Min(region.MinY, y); region.MaxY = Mathf.Max(region.MaxY, y);
                votes[(3 - Mathf.Min(3, y * 4 / height)) * 4 + Mathf.Min(3, x * 4 / width)]++;
                for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                    int next = ny * width + nx;
                    if (visited[next] || pixels[next].a < 96) continue;
                    visited[next] = true; queue.Enqueue(next);
                }
            }
            region.Cell = Array.IndexOf(votes, votes.Max()); result.Add(region);
        }
        return result;
    }

    private static Transform Joint(string name, Transform parent, float x, float y)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false); t.localPosition = new Vector3(x, y, 0);
        return t;
    }

    private static SpriteRenderer ArtNode(string name, Transform parent, Sprite sprite, float w, float h, int order)
    {
        var t = Joint(name, parent, 0, 0);
        var renderer = t.gameObject.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = order;
        t.localScale = new Vector3(w / sprite.bounds.size.x, h / sprite.bounds.size.y, 1);
        return renderer;
    }

    private static GameObject MakeRig(int character, Dictionary<string, Sprite> sprites)
    {
        bool mage = character == 1;
        var root = new GameObject(Names[character]);
        var animator = root.AddComponent<Animator>(); animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.applyRootMotion = false;
        root.AddComponent<AnimationLink>();
        var visual = Joint("Visual", root.transform, 0, 0);
        var hips = Joint("Hips", visual, 0, .325f);
        foreach (bool front in new[] { false, true })
        {
            string side = front ? "Front" : "Back";
            var thigh = Joint(side + "Leg", hips, front ? .018f : -.018f, 0);
            ArtNode("Art", thigh, sprites[side + "Thigh"], .19f, .21f, front ? 4 : 0);
            var shin = Joint("Shin", thigh, 0, -ThighLength);
            var sole = Joint("Sole", shin, 0, -ShinLength);
            ArtNode("Art", sole, sprites[side + "Shin"], .25f, .22f, front ? 5 : 1);
        }
        var upper = Joint("Upper", hips, 0, .07f);
        var body = Joint("Body", upper, 0, .20f);
        ArtNode("Art", body, sprites["Body"], mage ? .61f : .59f, .59f, 6);
        var head = Joint("Head", upper, .035f, .42f);
        ArtNode("Art", head, sprites["Head"], character == 0 ? 1.04f : .94f, .86f, 10);
        foreach (bool front in new[] { false, true })
        {
            string side = front ? "Front" : "Back";
            var arm = Joint(side + "Arm", upper, front ? .145f : .02f, .32f);
            ArtNode("Art", arm, sprites[side + "Arm"], .20f, .21f, front ? 11 : 2);
            var forearm = Joint("Forearm", arm, 0, -.155f);
            ArtNode("Art", forearm, sprites[side + "Hand"], .18f, .20f, front ? 14 : 3);
            if (front)
            {
                var weapon = Joint("Weapon", forearm, 0, -.13f);
                ArtNode("Art", weapon, sprites["Weapon"], character == 0 ? .13f : .17f, WeaponHeight(character), 13);
                if (mage)
                {
                    var lantern = Joint("Lantern", weapon, .085f, .72f);
                    ArtNode("Art", lantern, sprites["Accessory"], .19f, .27f, 15);
                }
            }
            else if (!mage)
            {
                var shield = Joint("Shield", forearm, .025f, -.13f);
                ArtNode("Art", shield, sprites["Accessory"], character == 2 ? .44f : .28f, character == 2 ? .54f : .34f, 15);
            }
        }
        var effects = Joint("Effects", root.transform, 0, 0);
        foreach (var name in new[] { "Trail", "Impact", "Projectile", "Dust" })
        {
            var t = Joint(name, effects, 0, 0);
            float w = name == "Trail" ? .34f : name == "Impact" ? .13f : name == "Dust" ? .18f : .24f;
            float h = name == "Trail" ? .09f : name == "Impact" ? .13f : name == "Dust" ? .065f : .095f;
            var r = ArtNode("Art", t, sprites[name], w, h, 30);
            r.color = new Color(1, 1, 1, 0);
        }
        ApplyPose(root, PoseAt(character, "Idle", 0));
        return root;
    }

    private static float WeaponHeight(int c) => c == 0 ? 1.16f : c == 1 ? 1.08f : .63f;

    private sealed class Pose
    {
        public int Character;
        public float X, Y, Roll, HipsY = .325f, Body = -1, Head, FrontLeg, FrontShin, BackLeg, BackShin;
        public float FrontFootPitch, BackFootPitch;
        public float FrontArm = 50, FrontElbow = -24, BackArm = 46, BackElbow = 16, WeaponAngle = -28, Accessory;
        public float Trail, Impact, Projectile, Dust, ProjectileDistance, DustX, FxScale = 1;
        public float Alpha = 1, Green = 1, Blue = 1;
        public Vector2 FrontFoot = new Vector2(.045f, 0), BackFoot = new Vector2(-.025f, 0);
        public bool TrackStrike, InFlight;
    }

    private static float K(float t, params float[] values)
    {
        float s = Mathf.Clamp01(t) * (values.Length - 1); int i = Mathf.Min((int)s, values.Length - 2);
        return Mathf.Lerp(values[i], values[i + 1], Mathf.SmoothStep(0, 1, s - i));
    }

    private static float Pulse(float t, float start, float peak, float end)
    {
        if (t <= start || t >= end) return 0;
        return t < peak ? Mathf.SmoothStep(0, 1, (t - start) / (peak - start)) : Mathf.SmoothStep(1, 0, (t - peak) / (end - peak));
    }

    private static Vector2 FootPath(float phase)
    {
        phase = Mathf.Repeat(phase, 1);
        if (phase <= .62f) return new Vector2(Mathf.Lerp(.09f, -.09f, phase / .62f), 0);
        float swing = (phase - .62f) / .38f;
        return new Vector2(Mathf.Lerp(-.09f, .09f, Mathf.SmoothStep(0, 1, swing)), .055f * Mathf.Sin(swing * Mathf.PI));
    }

    private static void SolveLeg(Vector2 foot, float hipX, Pose p, out float upper, out float lower)
    {
        Vector2 delta = foot - new Vector2(p.X + hipX, p.Y + p.HipsY);
        float d = Mathf.Clamp(delta.magnitude, .04f, ThighLength + ShinLength - .0001f);
        float direction = Mathf.Atan2(delta.x, -delta.y) * Mathf.Rad2Deg;
        float hip = Mathf.Acos(Mathf.Clamp((ThighLength * ThighLength + d * d - ShinLength * ShinLength) / (2 * ThighLength * d), -1, 1)) * Mathf.Rad2Deg;
        upper = direction + hip;
        lower = -Mathf.Acos(Mathf.Clamp((d * d - ThighLength * ThighLength - ShinLength * ShinLength) / (2 * ThighLength * ShinLength), -1, 1)) * Mathf.Rad2Deg;
    }

    private static Pose PoseAt(int c, string motion, float t)
    {
        var p = new Pose { Character = c, TrackStrike = motion == "Attack" && c != 1, WeaponAngle = c == 1 ? -22 : c == 2 ? -20 : -28 };
        float s = Mathf.Sin(t * Mathf.PI * 2), co = Mathf.Cos(t * Mathf.PI * 2);
        bool mage = c == 1;
        if (motion == "Idle" || motion == "AttackIdle")
        {
            p.HipsY += .003f * s; p.Body += .55f * s; p.Head = -.5f * p.Body;
            p.FrontArm += 1.2f * s; p.FrontElbow += .8f * s;
            p.BackArm += 1.4f * s; p.Accessory = 2 * s;
            if (motion == "AttackIdle") { p.FrontArm += 5; p.FrontElbow -= 6; p.WeaponAngle -= 8; p.Body -= 2; }
        }
        else if (motion == "Walk")
        {
            p.FrontFoot = FootPath(t); p.BackFoot = FootPath(t + .5f);
            float frontPhase = Mathf.Repeat(t, 1), backPhase = Mathf.Repeat(t + .5f, 1);
            p.FrontFootPitch = frontPhase > .62f ? -8 * Mathf.Sin((frontPhase - .62f) / .38f * Mathf.PI * 2) : 0;
            p.BackFootPitch = backPhase > .62f ? -8 * Mathf.Sin((backPhase - .62f) / .38f * Mathf.PI * 2) : 0;
            p.HipsY += .006f * (1 - Mathf.Cos(t * Mathf.PI * 4));
            p.Body = -2.2f + .6f * s; p.Head = -.65f * p.Body;
            // Equipped hands carry their load; only the free arm swings visibly.
            p.FrontArm = 50 + 2.5f * s; p.FrontElbow = -24 - 2 * s;
            p.BackArm = mage ? 22 - 8 * s : 46 + 2 * s;
            p.BackElbow = mage ? 6 + 2 * co : 16 - 2 * s;
            p.WeaponAngle += 1.4f * s; p.Accessory = 3 * s;
            float frontStrike = Pulse(Mathf.Repeat(t + .04f, 1), 0, .04f, .13f);
            float backStrike = Pulse(Mathf.Repeat(t + .54f, 1), 0, .04f, .13f);
            p.Dust = .32f * Mathf.Max(frontStrike, backStrike);
            p.DustX = (frontStrike >= backStrike ? p.FrontFoot.x : p.BackFoot.x) - .04f;
        }
        else if (motion == "Attack")
        {
            p.FrontFoot = new Vector2(.09f, 0); p.BackFoot = new Vector2(-.06f, 0);
            p.X = K(t, 0, -.01f, -.02f, .025f, .05f, .03f, .01f, 0, 0);
            p.HipsY -= K(t, 0, .002f, .005f, .010f, .013f, .008f, .003f, 0, 0);
            p.Body = K(t, -1, -2, -4, -7, -9, -5, -2, -1, -1); p.Head = -p.Body * .55f;
            if (c == 0)
            {
                p.FrontArm = K(t, 50, 46, 38, 62, 78, 72, 60, 52, 50);
                p.FrontElbow = K(t, -24, -34, -44, -16, -3, -10, -20, -24, -24);
                p.WeaponAngle = K(t, -28, -45, -70, -85, -90, -87, -55, -32, -28);
                p.BackArm = K(t, 46, 45, 43, 49, 53, 50, 47, 46, 46);
            }
            else if (mage)
            {
                p.X *= .3f;
                p.FrontArm = K(t, 50, 52, 55, 58, 68, 65, 58, 52, 50);
                p.FrontElbow = K(t, -24, -26, -28, -27, -18, -20, -23, -24, -24);
                p.BackArm = K(t, 22, 25, 32, 45, 62, 54, 38, 25, 22);
                p.BackElbow = K(t, 6, 10, 14, 16, 4, 8, 10, 8, 6);
                p.WeaponAngle = K(t, -22, -20, -18, -18, -27, -25, -23, -22, -22);
                p.Accessory = K(t, 0, 3, -3, 5, -6, 4, -2, 1, 0);
                p.Projectile = Pulse(t, .50f, .55f, .98f);
                p.ProjectileDistance = Mathf.Max(0, t - .5f) * 2.6f;
                p.InFlight = t >= .5f;
                p.Trail = .42f * Pulse(t, .15f, .40f, .52f);
            }
            else
            {
                p.FrontArm = K(t, 50, 62, 82, 88, 78, 64, 56, 51, 50);
                p.FrontElbow = K(t, -24, -5, 20, 12, -8, -23, -28, -25, -24);
                p.WeaponAngle = K(t, -20, 15, 38, -8, -100, -125, -70, -28, -20);
                p.BackArm = K(t, 46, 48, 51, 56, 60, 55, 50, 47, 46);
                p.BackElbow = K(t, 16, 16, 18, 20, 22, 20, 18, 16, 16);
            }
            if (!mage)
            {
                p.Trail = .68f * Pulse(t, .40f, .49f, .60f);
                // A short tip spark reads as weapon energy, rather than a floating hit on an absent enemy.
                p.Impact = .55f * Pulse(t, .49f, .52f, .59f);
            }
        }
        else if (motion == "Hurt")
        {
            float hit = K(t, 0, 1, .7f, .35f, .15f, .05f, 0, 0, 0);
            p.X = -.035f * hit; p.Body = 5 * hit; p.Head = -4 * hit;
            p.FrontArm += 7 * hit; p.BackArm -= 5 * hit;
            p.Green = 1 - .22f * hit; p.Blue = 1 - .22f * hit;
        }
        else if (motion == "Death")
        {
            float fall = K(t, 0, .03f, .15f, .40f, .75f, .95f, 1, 1, 1);
            p.Roll = 76 * fall; p.Y = .015f * Mathf.Sin(t * Mathf.PI);
            p.FrontArm += 12 * fall; p.FrontElbow -= 18 * fall;
            p.BackArm -= 10 * fall; p.Head = -8 * fall;
            p.Alpha = K(t, 1, 1, 1, 1, 1, 1, .8f, .4f, 0);
            p.Dust = .35f * Pulse(t, .48f, .62f, .80f); p.DustX = -.35f;
        }
        SolveLeg(p.FrontFoot, .018f, p, out p.FrontLeg, out p.FrontShin);
        SolveLeg(p.BackFoot, -.018f, p, out p.BackLeg, out p.BackShin);
        return p;
    }

    private static Vector2 HandPosition(Pose p)
    {
        return new Vector2(p.X, p.Y + p.HipsY + .07f)
            + Rotate(new Vector2(.145f, .32f), p.Body)
            + Rotate(new Vector2(0, -.155f), p.Body + p.FrontArm)
            + Rotate(new Vector2(0, -.13f), p.Body + p.FrontArm + p.FrontElbow);
    }

    private static Vector2 WeaponTip(Pose p) => HandPosition(p) + Rotate(new Vector2(0, WeaponHeight(p.Character) * .75f), p.Body + p.WeaponAngle);

    private static Dictionary<string, Vector3> Transforms(Pose p, bool mage)
    {
        var map = new Dictionary<string, Vector3> {
            { "Visual", new Vector3(p.X, p.Y, p.Roll) },
            { "Visual/Hips", new Vector3(0, p.HipsY, 0) },
            { "Visual/Hips/FrontLeg", new Vector3(.018f, 0, p.FrontLeg) },
            { "Visual/Hips/FrontLeg/Shin", new Vector3(0, -ThighLength, p.FrontShin) },
            { "Visual/Hips/FrontLeg/Shin/Sole", new Vector3(0, -ShinLength, -p.FrontLeg - p.FrontShin + p.FrontFootPitch) },
            { "Visual/Hips/BackLeg", new Vector3(-.018f, 0, p.BackLeg) },
            { "Visual/Hips/BackLeg/Shin", new Vector3(0, -ThighLength, p.BackShin) },
            { "Visual/Hips/BackLeg/Shin/Sole", new Vector3(0, -ShinLength, -p.BackLeg - p.BackShin + p.BackFootPitch) },
            { Upper, new Vector3(0, .07f, p.Body) },
            { Upper + "/Head", new Vector3(.035f, .42f, p.Head) },
            { FrontArm, new Vector3(.145f, .32f, p.FrontArm) },
            { FrontHand, new Vector3(0, -.155f, p.FrontElbow) },
            { BackArm, new Vector3(.02f, .32f, p.BackArm) },
            { BackHand, new Vector3(0, -.155f, p.BackElbow) },
            { Weapon, new Vector3(0, -.13f, p.WeaponAngle - p.FrontArm - p.FrontElbow) }
        };
        Vector2 hand = HandPosition(p), tip = WeaponTip(p);
        Vector2 trail = mage ? tip : Vector2.Lerp(hand, tip, .84f);
        Vector2 projectile = mage && p.InFlight ? WeaponTip(PoseAt(1, "Attack", .5f)) + new Vector2(p.ProjectileDistance, 0) : tip;
        map.Add("Effects/Trail", new Vector3(trail.x, trail.y, mage ? -10 : 90 + p.Body + p.WeaponAngle));
        map.Add("Effects/Impact", new Vector3(tip.x, tip.y, 0));
        map.Add("Effects/Projectile", new Vector3(projectile.x, projectile.y, 0));
        map.Add("Effects/Dust", new Vector3(p.DustX, .025f, 0));
        if (mage) map.Add(Weapon + "/Lantern", new Vector3(.085f, .72f, p.Accessory - p.WeaponAngle - p.Body));
        else map.Add(BackHand + "/Shield", new Vector3(.025f, -.13f, -p.BackArm - p.BackElbow + p.Accessory));
        return map;
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float a = degrees * Mathf.Deg2Rad;
        return new Vector2(v.x * Mathf.Cos(a) - v.y * Mathf.Sin(a), v.x * Mathf.Sin(a) + v.y * Mathf.Cos(a));
    }

    private static void ApplyPose(GameObject root, Pose pose)
    {
        foreach (var item in Transforms(pose, root.name == Names[1]))
        {
            var t = root.transform.Find(item.Key); t.localPosition = new Vector3(item.Value.x, item.Value.y, 0);
            t.localEulerAngles = new Vector3(0, 0, item.Value.z);
        }
    }

    private static AnimationClip MakeClip(int c, string motion, GameObject root)
    {
        bool loop = motion == "Idle" || motion == "Walk" || motion == "AttackIdle";
        float duration = motion == "Walk" ? WalkDuration(c) : motion == "Attack" ? (c == 1 ? 1.05f : c == 2 ? .88f : .72f) : motion == "Hurt" ? .4f : motion == "Death" ? .95f : 1.4f;
        var clip = new AnimationClip { name = motion, frameRate = 60 };
        var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        var poses = Enumerable.Range(0, 65).Select(n => PoseAt(c, motion, n / 64f)).ToArray();
        foreach (string path in Transforms(poses[0], c == 1).Keys)
        {
            var values = poses.Select(p => Transforms(p, c == 1)[path]).ToArray();
            Curve(clip, path, typeof(Transform), "m_LocalPosition.x", duration, values.Select(v => v.x).ToArray());
            Curve(clip, path, typeof(Transform), "m_LocalPosition.y", duration, values.Select(v => v.y).ToArray());
            Curve(clip, path, typeof(Transform), "localEulerAnglesRaw.z", duration, values.Select(v => v.z).ToArray());
        }
        foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>())
        {
            string path = AnimationUtility.CalculateTransformPath(renderer.transform, root.transform);
            bool effect = path.StartsWith("Effects/", StringComparison.Ordinal);
            Func<Pose, float> alpha = p => !effect ? p.Alpha : path.Contains("/Trail/") ? p.Trail : path.Contains("/Impact/") ? p.Impact : path.Contains("/Projectile/") ? p.Projectile : p.Dust;
            Curve(clip, path, typeof(SpriteRenderer), "m_Color.a", duration, poses.Select(alpha).ToArray());
            Curve(clip, path, typeof(SpriteRenderer), "m_Color.r", duration, poses.Select(p => 1f).ToArray());
            Curve(clip, path, typeof(SpriteRenderer), "m_Color.g", duration, poses.Select(p => effect ? 1 : p.Green).ToArray());
            Curve(clip, path, typeof(SpriteRenderer), "m_Color.b", duration, poses.Select(p => effect ? 1 : p.Blue).ToArray());
        }
        foreach (string effect in new[] { "Trail", "Impact", "Projectile", "Dust" })
        {
            string path = "Effects/" + effect;
            var scale = poses.Select(p => p.FxScale).ToArray();
            Curve(clip, path, typeof(Transform), "m_LocalScale.x", duration, scale);
            Curve(clip, path, typeof(Transform), "m_LocalScale.y", duration, scale);
        }
        // The guard lifts its club behind its head during the wind-up, then strikes in front.
        // Every state keys the order so an interrupted attack cannot leave the weapon behind.
        bool behindHead = c == 2 && motion == "Attack";
        var orderCurve = behindHead
            ? new AnimationCurve(new Keyframe(0, 13), new Keyframe(duration * .10f, 7),
                new Keyframe(duration * .40f, 13), new Keyframe(duration, 13))
            : new AnimationCurve(new Keyframe(0, 13), new Keyframe(duration, 13));
        for (int n = 0; n < orderCurve.length; n++)
        {
            AnimationUtility.SetKeyLeftTangentMode(orderCurve, n, AnimationUtility.TangentMode.Constant);
            AnimationUtility.SetKeyRightTangentMode(orderCurve, n, AnimationUtility.TangentMode.Constant);
        }
        AnimationUtility.SetEditorCurve(clip,
            EditorCurveBinding.FloatCurve(Weapon + "/Art", typeof(SpriteRenderer), "m_SortingOrder"), orderCurve);
        if (motion == "Attack") AnimationUtility.SetAnimationEvents(clip, new[] {
            new AnimationEvent { time = duration * .5f, functionName = "Fire" },
            new AnimationEvent { time = duration, functionName = "AniEnd" }
        });
        return clip;
    }

    private static void Curve(AnimationClip clip, string path, Type type, string property, float duration, float[] values)
    {
        // Unity serializes both editable and runtime curves. Keep constant channels to two keys.
        if (values.All(v => Mathf.Abs(v - values[0]) < .00001f)) values = new[] { values[0], values[0] };
        var keys = new Keyframe[values.Length];
        for (int n = 0; n < keys.Length; n++) keys[n] = new Keyframe(duration * n / (keys.Length - 1), values[n]);
        var curve = new AnimationCurve(keys);
        for (int n = 0; n < keys.Length; n++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, n, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(curve, n, AnimationUtility.TangentMode.ClampedAuto);
        }
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, type, property), curve);
    }

    [MenuItem("Tools/Renewal Characters/Validate animations")]
    public static void Validate()
    {
        int samples = 0;
        foreach (string name in Names)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + name + ".prefab");
            if (prefab == null) throw new InvalidOperationException("Missing prefab: " + name);
            var animator = prefab.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null || prefab.GetComponent<AnimationLink>() == null)
                throw new InvalidOperationException("Missing animation components: " + name);
            var instance = UnityEngine.Object.Instantiate(prefab); instance.GetComponent<Animator>().fireEvents = false;
            try
            {
                var clips = animator.runtimeAnimatorController.animationClips.ToDictionary(c => c.name);
                if (clips.Count != 6) throw new InvalidOperationException("Expected six motions: " + name);
                var liveAnimator = instance.GetComponent<Animator>();
                liveAnimator.Rebind();
                liveAnimator.Update(0);
                foreach (string state in Motions)
                {
                    liveAnimator.Play(state, 0, 0);
                    liveAnimator.Update(.03f);
                    if (liveAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash != Animator.StringToHash(state))
                        throw new InvalidOperationException("Animator cannot enter state: " + name + "/" + state);
                }
                liveAnimator.enabled = false;
                var renderers = instance.GetComponentsInChildren<SpriteRenderer>();
                if (renderers.Length < 15 || renderers.Any(r => r.sprite == null)) throw new InvalidOperationException("Missing cutout parts: " + name);
                foreach (var clip in clips.Values)
                {
                    bool loop = clip.name == "Idle" || clip.name == "Walk" || clip.name == "AttackIdle";
                    if (clip.isLooping != loop || clip.length <= 0) throw new InvalidOperationException("Invalid clip: " + clip.name);
                    var bindings = AnimationUtility.GetCurveBindings(clip);
                    foreach (var binding in bindings)
                        if (instance.transform.Find(binding.path) == null) throw new InvalidOperationException("Unbound curve: " + binding.path);
                    bool armMoves = false, legMoves = false, weaponMoves = false, visibleFx = false;
                    clip.SampleAnimation(instance, 0);
                    float arm = instance.transform.Find(FrontArm).localEulerAngles.z;
                    float leg = instance.transform.Find("Visual/Hips/FrontLeg").localEulerAngles.z;
                    float weapon = instance.transform.Find(Weapon).eulerAngles.z;
                    var first = instance.GetComponentsInChildren<Transform>().Select(t => Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale)).ToArray();
                    for (int n = 1; n <= 64; n++)
                    {
                        clip.SampleAnimation(instance, clip.length * n / 64f); samples++;
                        armMoves |= Mathf.Abs(Mathf.DeltaAngle(arm, instance.transform.Find(FrontArm).localEulerAngles.z)) > 2;
                        legMoves |= Mathf.Abs(Mathf.DeltaAngle(leg, instance.transform.Find("Visual/Hips/FrontLeg").localEulerAngles.z)) > 2;
                        weaponMoves |= Mathf.Abs(Mathf.DeltaAngle(weapon, instance.transform.Find(Weapon).eulerAngles.z)) > .5f;
                        visibleFx |= instance.transform.Find("Effects").GetComponentsInChildren<SpriteRenderer>().Any(r => r.color.a > .25f);
                        if (instance.transform.localPosition != Vector3.zero || instance.transform.localScale != Vector3.one)
                            throw new InvalidOperationException("Gameplay root animated: " + clip.name);
                    }
                    if ((clip.name == "Walk" || clip.name == "Attack") && (!armMoves || !legMoves || !weaponMoves))
                        throw new InvalidOperationException("Missing independent limb/weapon motion: " + name + "/" + clip.name);
                    if (clip.name == "Attack" && !visibleFx) throw new InvalidOperationException("Missing attack effect: " + name);
                    if (loop)
                    {
                        var last = instance.GetComponentsInChildren<Transform>();
                        for (int n = 1; n < last.Length; n++)
                            for (int m = 0; m < 16; m++)
                                if (Mathf.Abs(first[n][m] - Matrix4x4.TRS(last[n].localPosition, last[n].localRotation, last[n].localScale)[m]) > .001f)
                                    throw new InvalidOperationException("Loop seam: " + name + "/" + clip.name);
                    }
                }
                ValidateFootContacts(instance, clips["Walk"], Array.IndexOf(Names, name));
                clips["Attack"].SampleAnimation(instance, clips["Attack"].length * .5f);
                clips["Death"].SampleAnimation(instance, clips["Death"].length);
                clips["Idle"].SampleAnimation(instance, 0);
                if (renderers.Any(r => !r.transform.IsChildOf(instance.transform.Find("Effects")) && r.color.a < .99f))
                    throw new InvalidOperationException("Death opacity leaked into Idle: " + name);
                if (instance.transform.Find("Effects").GetComponentsInChildren<SpriteRenderer>().Any(r => r.color.a > .001f))
                    throw new InvalidOperationException("Effect leaked into Idle: " + name);
                if (instance.transform.Find(Weapon + "/Art").GetComponent<SpriteRenderer>().sortingOrder != 13)
                    throw new InvalidOperationException("Weapon depth leaked into Idle: " + name);
                if (name == Names[2])
                {
                    clips["Attack"].SampleAnimation(instance, clips["Attack"].length * .25f);
                    if (instance.transform.Find(Weapon + "/Art").GetComponent<SpriteRenderer>().sortingOrder != 7)
                        throw new InvalidOperationException("Guard club must wind up behind its head.");
                }
                var events = AnimationUtility.GetAnimationEvents(clips["Attack"]);
                if (events.Length != 2 || events[0].functionName != "Fire" || events[1].functionName != "AniEnd")
                    throw new InvalidOperationException("Missing attack callbacks: " + name);
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }
        Debug.Log("RENEWAL_RIGS_OK: 3 articulated rigs, 48 atlas parts, 18 clips, " + samples + " sampled poses. Right-facing gait, planted soles, independent limbs, weapons, restrained VFX, loop seams and state resets verified.");
    }

    private static void ValidateFootContacts(GameObject instance, AnimationClip walk, int character)
    {
        for (int n = 0; n <= 128; n++)
        {
            float t = n / 128f; walk.SampleAnimation(instance, walk.length * t);
            for (int side = 0; side < 2; side++)
            {
                string part = side == 0 ? "Front" : "Back";
                var sole = instance.transform.Find("Visual/Hips/" + part + "Leg/Shin/Sole");
                Vector2 actual = instance.transform.InverseTransformPoint(sole.position);
                var expected = FootPath(t + (side == 0 ? 0 : .5f));
                if (Vector2.Distance(actual, expected) > .006f || actual.y < -.003f)
                    throw new InvalidOperationException("Foot slips off its gait path: " + Names[character] + "/" + part + " at " + t);
                if (expected.y < .0001f && Mathf.Abs(Mathf.DeltaAngle(0, sole.eulerAngles.z)) > 2)
                    throw new InvalidOperationException("Planted boot is not flat: " + Names[character]);
            }
        }
    }

    [MenuItem("Tools/Renewal Characters/Validate preview scene")]
    public static void ValidatePreview()
    {
        const string path = "Assets/01_Scenes/RenewalCharacterPreview.unity";
        var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
        bool opened = !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        try
        {
            bool found = false;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var script in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (script == null) throw new InvalidOperationException("Missing preview scene script.");
                var serialized = new SerializedObject(script);
                var entries = serialized.FindProperty("characters");
                if (entries == null) continue;
                found = true;
                if (!serialized.FindProperty("showExtendedMotions").boolValue || entries.arraySize != 3)
                    throw new InvalidOperationException("Expected three characters and six motion buttons.");
                for (int n = 0; n < entries.arraySize; n++)
                    if (entries.GetArrayElementAtIndex(n).FindPropertyRelative("prefab").objectReferenceValue == null)
                        throw new InvalidOperationException("Broken preview prefab reference.");
            }
            if (!found) throw new InvalidOperationException("Missing CharacterGallery in preview scene.");
            Debug.Log("RENEWAL_PREVIEW_OK: scene script, 3 prefab references and extended motion controls verified.");
        }
        finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
    }

    // Render actual Unity poses for visual review, without opening or changing the user's scene.
    public static void RenderReview()
    {
        string output = Path.Combine(Directory.GetCurrentDirectory(), "Review"); Directory.CreateDirectory(output);
        var cameraObject = new GameObject("Review camera"); var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = 1.12f;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.11f, .15f, .20f, 1);
        var rt = new RenderTexture(512, 512, 24); camera.targetTexture = rt;
        var atlas = new Texture2D(512 * 6, 512 * 3, TextureFormat.RGB24, false);
        try
        {
            for (int c = 0; c < Names.Length; c++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + Names[c] + ".prefab");
                var instance = UnityEngine.Object.Instantiate(prefab); instance.transform.position = new Vector3(c * 20, 0, 0);
                var animator = instance.GetComponent<Animator>(); animator.enabled = false;
                var clips = animator.runtimeAnimatorController.animationClips.ToDictionary(a => a.name);
                camera.transform.position = new Vector3(c * 20 + .35f, 1.1f, -10);
                string[] states = { "Idle", "Walk", "Attack", "Attack", "Hurt", "Death" };
                float[] times = { 0, .25f, .25f, .53f, .15f, .5f };
                for (int n = 0; n < states.Length; n++)
                {
                    var clip = clips[states[n]]; clip.SampleAnimation(instance, clip.length * times[n]);
                    camera.transform.position = new Vector3(c * 20 + (states[n] == "Death" ? -.40f : states[n] == "Attack" ? .45f : .15f), states[n] == "Death" ? .50f : .80f, -10);
                    camera.Render(); RenderTexture.active = rt;
                    atlas.ReadPixels(new Rect(0, 0, 512, 512), n * 512, (2 - c) * 512, false);
                }
                UnityEngine.Object.DestroyImmediate(instance);
            }
            atlas.Apply(); File.WriteAllBytes(Path.Combine(output, "ArticulatedPoses.png"), atlas.EncodeToPNG());
            Debug.Log("RENEWAL_REVIEW_OK: " + output);
        }
        finally
        {
            RenderTexture.active = null; camera.targetTexture = null; rt.Release();
            UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(atlas); UnityEngine.Object.DestroyImmediate(cameraObject);
        }
    }

    public static void RenderWalkReview()
    {
        string output = Path.Combine(Directory.GetCurrentDirectory(), "Review"); Directory.CreateDirectory(output);
        var cameraObject = new GameObject("Walk review camera"); var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = .98f;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.11f, .15f, .20f, 1);
        var rt = new RenderTexture(384, 384, 24); camera.targetTexture = rt;
        var atlas = new Texture2D(384 * 8, 384 * 3, TextureFormat.RGB24, false);
        var groundObject = new GameObject("Sole baseline");
        var ground = groundObject.AddComponent<LineRenderer>();
        ground.material = new Material(Shader.Find("Sprites/Default"));
        ground.positionCount = 2; ground.startWidth = ground.endWidth = .008f;
        ground.startColor = ground.endColor = new Color(.30f, .39f, .44f, 1);
        try
        {
            for (int c = 0; c < Names.Length; c++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + Names[c] + ".prefab");
                var instance = UnityEngine.Object.Instantiate(prefab); instance.transform.position = new Vector3(c * 20, 0, 0);
                var animator = instance.GetComponent<Animator>(); animator.enabled = false;
                var walk = animator.runtimeAnimatorController.animationClips.First(a => a.name == "Walk");
                camera.transform.position = new Vector3(c * 20 + .15f, .76f, -10);
                ground.SetPosition(0, new Vector3(c * 20 - 2, -.008f, .1f));
                ground.SetPosition(1, new Vector3(c * 20 + 2, -.008f, .1f));
                for (int n = 0; n < 8; n++)
                {
                    walk.SampleAnimation(instance, walk.length * n / 8f);
                    camera.Render(); RenderTexture.active = rt;
                    atlas.ReadPixels(new Rect(0, 0, 384, 384), n * 384, (2 - c) * 384, false);
                }
                UnityEngine.Object.DestroyImmediate(instance);
            }
            atlas.Apply(); File.WriteAllBytes(Path.Combine(output, "RightFacingWalkCycle.png"), atlas.EncodeToPNG());
            Debug.Log("RENEWAL_WALK_REVIEW_OK: " + output);
        }
        finally
        {
            RenderTexture.active = null; camera.targetTexture = null; rt.Release();
            UnityEngine.Object.DestroyImmediate(ground.sharedMaterial); UnityEngine.Object.DestroyImmediate(groundObject);
            UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(atlas); UnityEngine.Object.DestroyImmediate(cameraObject);
        }
    }

    public static void BuildAndReview() { Build(); ValidatePreview(); RenderReview(); RenderWalkReview(); }
    public static void ValidateAll() { Validate(); ValidatePreview(); }
}
