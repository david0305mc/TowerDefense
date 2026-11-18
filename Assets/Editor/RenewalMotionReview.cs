using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Exports the actual Unity clips against a fixed ground reference for motion review.</summary>
public static class RenewalMotionReview
{
    private static readonly string[] Names = { "Mushroom_Spearguard", "Lantern_Mage", "Acorn_Shieldguard" };
    private const string Prefabs = "Assets/03_Prefabs/InGame/Character/Renewal/";

    public static void BuildAndExport()
    {
        RenewalAnimationBuilder.BuildAndReview();
        Export();
    }

    [MenuItem("Tools/Renewal Characters/Export motion review")]
    public static void Export()
    {
        string folder = Path.Combine(Directory.GetCurrentDirectory(), "Review", "Motion");
        Directory.CreateDirectory(folder);
        var review = new GameObject("Motion review");
        var camera = new GameObject("Motion camera").AddComponent<Camera>();
        camera.transform.SetParent(review.transform);
        camera.transform.position = new Vector3(.2f, 2.82f, -10);
        camera.orthographic = true; camera.orthographicSize = 3.35f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.10f, .135f, .18f, 1);
        var target = new RenderTexture(768, 512, 24);
        var frame = new Texture2D(768, 512, TextureFormat.RGB24, false);
        var lineMaterial = new Material(Shader.Find("Sprites/Default"));
        camera.targetTexture = target;
        var objects = new GameObject[3];
        var clips = new AnimationClip[3][];
        try
        {
            for (int i = 0; i < Names.Length; i++)
            {
                float floor = (2 - i) * 2;
                objects[i] = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + Names[i] + ".prefab"), review.transform);
                var animator = objects[i].GetComponent<Animator>();
                animator.enabled = false;
                clips[i] = animator.runtimeAnimatorController.animationClips;
                Label(review.transform, Names[i].Split('_')[0].ToUpperInvariant(), -4.4f, floor + .8f, .08f);
                Line(review.transform, lineMaterial, new Vector3(-2.8f, floor - .015f), new Vector3(4.1f, floor - .015f), .012f, new Color(.32f, .40f, .45f));
                for (float x = -2.5f; x < 4; x += .5f)
                    Line(review.transform, lineMaterial, new Vector3(x, floor - .02f), new Vector3(x, floor - .08f), .008f, new Color(.24f, .31f, .36f));
            }
            var heading = Label(review.transform, "RIGHT-FACING WALK", -4.4f, 5.9f, .10f);
            const int fps = 16;
            for (int mode = 0; mode < 2; mode++)
            {
                string state = mode == 0 ? "Walk" : "Attack";
                heading.text = mode == 0 ? "RIGHT-FACING WALK   /   LEFT TO RIGHT" : "RIGHT-FACING ATTACK   /   SHORT VFX";
                string frames = Path.Combine(folder, state); Directory.CreateDirectory(frames);
                int count = mode == 0 ? 64 : 32;
                for (int n = 0; n < count; n++)
                {
                    float elapsed = n / (float)fps;
                    for (int i = 0; i < objects.Length; i++)
                    {
                        var clip = clips[i].Single(c => c.name == state);
                        if (mode == 0)
                        {
                            clip.SampleAnimation(objects[i], elapsed % clip.length);
                            objects[i].transform.localPosition = new Vector3(-.9f + RenewalAnimationBuilder.PreviewWalkSpeed(i) * elapsed, (2 - i) * 2, 0);
                        }
                        else
                        {
                            float sample = elapsed < .25f ? 0 : Mathf.Min(elapsed - .25f, clip.length);
                            clip.SampleAnimation(objects[i], sample);
                            objects[i].transform.localPosition = new Vector3(-.9f, (2 - i) * 2, 0);
                        }
                    }
                    camera.Render(); RenderTexture.active = target;
                    frame.ReadPixels(new Rect(0, 0, 768, 512), 0, 0); frame.Apply();
                    File.WriteAllBytes(Path.Combine(frames, n.ToString("D3") + ".png"), frame.EncodeToPNG());
                }
            }
            Debug.Log("RENEWAL_MOTION_REVIEW_OK: " + folder + " (16fps;64 walk frames and32 attack frames)");
        }
        finally
        {
            RenderTexture.active = null; camera.targetTexture = null; target.Release();
            UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(frame);
            UnityEngine.Object.DestroyImmediate(lineMaterial); UnityEngine.Object.DestroyImmediate(review);
        }
    }

    private static void Line(Transform parent, Material material, Vector3 a, Vector3 b, float width, Color color)
    {
        var line = new GameObject("Ground reference").AddComponent<LineRenderer>();
        line.transform.SetParent(parent, false); line.sharedMaterial = material;
        line.positionCount = 2; line.SetPosition(0, a); line.SetPosition(1, b);
        line.startWidth = line.endWidth = width; line.startColor = line.endColor = color;
        line.sortingOrder = -50;
    }

    private static TextMesh Label(Transform parent, string text, float x, float y, float size)
    {
        var label = new GameObject("Label").AddComponent<TextMesh>();
        label.transform.SetParent(parent, false); label.transform.localPosition = new Vector3(x, y, -1);
        label.text = text; label.fontSize = 48; label.characterSize = size;
        label.anchor = TextAnchor.MiddleLeft; label.color = new Color(.75f, .82f, .85f);
        label.GetComponent<MeshRenderer>().sortingOrder = 100;
        return label;
    }
}
