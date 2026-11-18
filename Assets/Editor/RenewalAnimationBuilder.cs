using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>Rebuilds the renewal samples as native, editable Unity animation assets.</summary>
public static class RenewalAnimationBuilder
{
    private const string Art = "Assets/04_Images/Renewal/";
    private const string Output = "Assets/07_Animations/Character/Renewal";
    private const string Prefabs = "Assets/03_Prefabs/InGame/Character/Renewal/";
    private static readonly string[] Names = { "Mushroom_Spearguard", "Lantern_Mage", "Acorn_Shieldguard" };
    private static readonly string[] Motions = { "Idle", "Walk", "Attack", "AttackIdle" };

    [MenuItem("Tools/Renewal Characters/Rebuild animations")]
    public static void Build()
    {
        Directory.CreateDirectory(Output);
        Directory.CreateDirectory(Prefabs);
        AssetDatabase.Refresh();
        for (int i = 0; i < Names.Length; i++)
        {
            string name = Names[i];
            string folder = Output + "/" + name;
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            var clips = new AnimationClip[Motions.Length];
            for (int j = 0; j < Motions.Length; j++)
            {
                string path = folder + "/" + Motions[j] + ".anim";
                AnimationClip generated = MakeClip(i, Motions[j]);
                clips[j] = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clips[j] == null)
                {
                    clips[j] = generated;
                    AssetDatabase.CreateAsset(clips[j], path);
                }
                else
                {
                    EditorUtility.CopySerialized(generated, clips[j]);
                    UnityEngine.Object.DestroyImmediate(generated);
                    EditorUtility.SetDirty(clips[j]);
                }
            }
            string controllerPath = folder + "/" + name + ".controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine = controller.layers[0].stateMachine;
            foreach (var state in machine.states) machine.RemoveState(state.state);
            for (int j = 0; j < Motions.Length; j++)
            {
                var state = machine.AddState(Motions[j], new Vector3(240, 60 + j * 70, 0));
                state.motion = clips[j];
                state.writeDefaultValues = false;
                if (j == 0) machine.defaultState = state;
            }
            EditorUtility.SetDirty(controller);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
            if (sprite == null) throw new InvalidOperationException("Missing sprite: " + name);
            var root = new GameObject(name);
            try
            {
                var visual = new GameObject("Visual");
                visual.transform.SetParent(root.transform, false);
                visual.AddComponent<SpriteRenderer>().sprite = sprite;
                var animator = root.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                root.AddComponent<AnimationLink>();
                PrefabUtility.SaveAsPrefabAsset(root, Prefabs + name + ".prefab");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Validate();
    }

    private static AnimationClip MakeClip(int character, string motion)
    {
        bool mage = character == 1;
        bool guard = character == 2;
        float duration = motion == "Idle" ? (mage ? 1.4f : 1.1f)
            : motion == "Walk" ? (mage ? 0.8f : guard ? 0.64f : 0.48f)
            : motion == "Attack" ? (mage ? 0.8f : guard ? 0.66f : 0.52f) : 0.8f;
        var clip = new AnimationClip { name = motion, frameRate = 60 };
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = motion != "Attack";
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        float[] x = { 0, 0, 0, 0, 0 };
        float[] y = { 0, 0, 0, 0, 0 };
        float[] rotation = { 0, 0, 0, 0, 0 };
        float[] sx = { 1, 1, 1, 1, 1 };
        float[] sy = { 1, 1, 1, 1, 1 };
        if (motion == "Idle")
        {
            y = mage ? new[] { 0.04f, 0.10f, 0.16f, 0.10f, 0.04f } : new[] { 0f, 0.01f, 0f, 0.01f, 0f };
            sx = new[] { 1f, 0.99f, 1f, 1.01f, 1f };
            sy = new[] { 1f, 1.025f, 1f, 0.985f, 1f };
            rotation = mage ? new[] { -2f, 0f, 2f, 0f, -2f } : rotation;
        }
        else if (motion == "Walk")
        {
            y = mage ? new[] { 0.12f, 0.22f, 0.12f, 0.22f, 0.12f }
                : new[] { 0f, guard ? 0.10f : 0.20f, 0f, guard ? 0.10f : 0.20f, 0f };
            rotation = mage ? new[] { -3f, -7f, -3f, 1f, -3f } : new[] { -5f, 0f, 5f, 0f, -5f };
            sx = new[] { 1.07f, 0.96f, 1.07f, 0.96f, 1.07f };
            sy = new[] { 0.94f, 1.06f, 0.94f, 1.06f, 0.94f };
        }
        else if (motion == "Attack")
        {
            x = mage ? new[] { 0f, -0.10f, 0.08f, 0.04f, 0f }
                : new[] { 0f, -0.16f, guard ? 0.30f : 0.42f, 0.12f, 0f };
            y = mage ? new[] { 0f, 0.20f, 0.28f, 0.12f, 0f } : new[] { 0f, 0f, 0.06f, 0f, 0f };
            rotation = mage ? new[] { 0f, 8f, -10f, -3f, 0f } : new[] { 0f, 10f, -14f, -5f, 0f };
            sx = new[] { 1f, 0.94f, 1.10f, 1.03f, 1f };
            sy = new[] { 1f, 1.04f, 0.93f, 0.98f, 1f };
            // Compatible with the existing AnimationLink / MBaseObj attack callbacks.
            AnimationUtility.SetAnimationEvents(clip, new[] {
                new AnimationEvent { time = duration * 0.5f, functionName = "Fire" },
                new AnimationEvent { time = duration, functionName = "AniEnd" }
            });
        }
        else
        {
            x = new[] { 0f, -0.015f, 0f, 0.015f, 0f };
            y = mage ? new[] { 0.08f, 0.12f, 0.08f, 0.04f, 0.08f } : y;
            rotation = new[] { -3f, -4f, -3f, -2f, -3f };
            sx = new[] { 1.03f, 1.02f, 1.03f, 1.04f, 1.03f };
            sy = new[] { 0.98f, 1f, 0.98f, 0.96f, 0.98f };
        }
        SetCurve(clip, "m_LocalPosition.x", duration, x);
        SetCurve(clip, "m_LocalPosition.y", duration, y);
        SetCurve(clip, "m_LocalPosition.z", duration, new[] { 0f, 0f, 0f, 0f, 0f });
        SetCurve(clip, "localEulerAnglesRaw.z", duration, rotation);
        SetCurve(clip, "m_LocalScale.x", duration, sx);
        SetCurve(clip, "m_LocalScale.y", duration, sy);
        SetCurve(clip, "m_LocalScale.z", duration, new[] { 1f, 1f, 1f, 1f, 1f });
        return clip;
    }

    private static void SetCurve(AnimationClip clip, string property, float duration, float[] values)
    {
        var keys = new Keyframe[values.Length];
        for (int i = 0; i < keys.Length; i++) keys[i] = new Keyframe(duration * i / (keys.Length - 1), values[i]);
        var curve = new AnimationCurve(keys);
        for (int i = 0; i < keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
        }
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("Visual", typeof(Transform), property), curve);
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
                throw new InvalidOperationException("Missing animator or callbacks: " + name);
            var instance = UnityEngine.Object.Instantiate(prefab);
            try
            {
                var visual = instance.transform.Find("Visual");
                foreach (var clip in animator.runtimeAnimatorController.animationClips)
                {
                    if (clip.length <= 0) throw new InvalidOperationException("Empty clip: " + name + "/" + clip.name);
                    if (clip.isLooping != (clip.name != "Attack")) throw new InvalidOperationException("Incorrect looping: " + clip.name);
                    clip.SampleAnimation(instance, 0);
                    var initial = Matrix4x4.TRS(visual.localPosition, visual.localRotation, visual.localScale);
                    bool changed = false;
                    for (int step = 1; step <= 8; step++)
                    {
                        clip.SampleAnimation(instance, clip.length * step / 8f);
                        var matrix = Matrix4x4.TRS(visual.localPosition, visual.localRotation, visual.localScale);
                        changed |= matrix != initial;
                        if (instance.transform.localPosition != Vector3.zero || instance.transform.localScale != Vector3.one)
                            throw new InvalidOperationException("Animation modified the gameplay root: " + clip.name);
                        if (visual.localScale.x <= 0 || visual.localScale.y <= 0)
                            throw new InvalidOperationException("Invalid sprite scale: " + clip.name);
                        samples++;
                    }
                    if (!changed) throw new InvalidOperationException("Static clip: " + clip.name);
                    if (clip.isLooping && Matrix4x4.TRS(visual.localPosition, visual.localRotation, visual.localScale) != initial)
                        throw new InvalidOperationException("Loop seam: " + clip.name);
                    if (clip.name == "Attack")
                    {
                        var events = AnimationUtility.GetAnimationEvents(clip);
                        if (events.Length != 2 || events[0].functionName != "Fire" || events[1].functionName != "AniEnd")
                            throw new InvalidOperationException("Missing attack events: " + name);
                    }
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }
        Debug.Log("RENEWAL_ANIMATIONS_OK: 3 prefabs, 12 clips, " + samples + " motion samples; loops and attack callbacks verified.");
    }
}
