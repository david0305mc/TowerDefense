using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Standalone visual previews: no combat, navigation, login or game managers.</summary>
public sealed class CharacterGallery : MonoBehaviour
{
    [Serializable]
    public sealed class Entry
    {
        public GameObject prefab;
        public string category;
    }

    public Entry[] characters = Array.Empty<Entry>();
    public bool showExtendedMotions;
    private readonly List<Entry> filtered = new List<Entry>();
    private readonly List<Animator> animators = new List<Animator>();
    private readonly List<Transform> models = new List<Transform>();
    private Transform previewRoot;
    private Camera view;
    private string search = "";
    private int category;
    private int page;
    private int motion;
    private float speed = 1;
    private bool paused;
    private bool flipped;
    private bool overview;
    private Vector3 home;
    private float homeSize;
    private Vector3 dragStart;
    private int screenWidth;
    private int screenHeight;
    private const int PageSize = 12;
    private static readonly string[] Categories = { "All", "Heroes", "Enemies", "Objects", "Extras" };
    private static readonly string[] BasicMotions = { "Idle", "Walk", "Attack", "AttackIdle" };
    private static readonly string[] ExtendedMotions = { "Idle", "Walk", "Attack", "AttackIdle", "Hurt", "Death" };
    private string[] Motions => showExtendedMotions ? ExtendedMotions : BasicMotions;

    private void Start()
    {
        view = GetComponentInChildren<Camera>();
        Rebuild();
    }

    private void Rebuild()
    {
        if (previewRoot != null)
        {
            previewRoot.gameObject.SetActive(false);
            Destroy(previewRoot.gameObject);
        }
        animators.Clear();
        models.Clear();
        filtered.Clear();
        foreach (var entry in characters)
        {
            if (entry.prefab == null) continue;
            if (category != 0 && entry.category != Categories[category]) continue;
            if (entry.prefab.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
            filtered.Add(entry);
        }
        page = Mathf.Clamp(page, 0, Mathf.Max(0, (filtered.Count - 1) / PageSize));
        previewRoot = new GameObject("Character previews").transform;
        // An inactive parent prevents Awake/OnEnable on gameplay scripts before removal.
        previewRoot.gameObject.SetActive(false);
        int start = overview ? 0 : page * PageSize;
        int count = overview ? filtered.Count : Mathf.Min(PageSize, filtered.Count - start);
        int columns = overview ? 8 : 4;
        for (int i = 0; i < count; i++)
        {
            var entry = filtered[start + i];
            var cell = new GameObject(entry.prefab.name).transform;
            cell.SetParent(previewRoot, false);
            cell.localPosition = new Vector3((i % columns) * 5, -(i / columns) * 4.5f, 0);
            var model = Instantiate(entry.prefab, cell, false);
            model.name = entry.prefab.name;
            // Remove scripts while inactive, including animation callbacks and state machines.
            foreach (var behaviour in model.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour != null) DestroyImmediate(behaviour);
            foreach (var agent in model.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>(true))
                DestroyImmediate(agent);
            foreach (var body in model.GetComponentsInChildren<Rigidbody2D>(true))
                body.simulated = false;
            foreach (var collider in model.GetComponentsInChildren<Collider2D>(true))
                collider.enabled = false;
            foreach (var collider in model.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
            foreach (var canvas in model.GetComponentsInChildren<Canvas>(true))
                canvas.gameObject.SetActive(false);
            foreach (var audio in model.GetComponentsInChildren<AudioSource>(true))
                audio.enabled = false;
            foreach (var animator in model.GetComponentsInChildren<Animator>(true))
            {
                animator.fireEvents = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animators.Add(animator);
            }
            model.SetActive(true);
            model.transform.localPosition = Vector3.zero;
            models.Add(model.transform);
            // Fit each visual into its cell without changing the source prefab.
            var renderers = model.GetComponentsInChildren<SpriteRenderer>();
            bool hasBounds = false;
            Bounds bounds = new Bounds();
            foreach (var renderer in renderers)
            {
                if (!renderer.enabled || !renderer.gameObject.activeSelf) continue;
                if (renderer.sprite == null) continue;
                if (renderer.color.a <= 0.001f) continue;
                // Sprite bounds also work before activating renderers for the first time.
                var spriteBounds = renderer.sprite.bounds;
                for (int corner = 0; corner < 4; corner++)
                {
                    var point = spriteBounds.center + new Vector3(
                        (corner % 2 == 0 ? -1 : 1) * spriteBounds.extents.x,
                        (corner < 2 ? -1 : 1) * spriteBounds.extents.y, 0);
                    point = renderer.transform.TransformPoint(point);
                    if (!hasBounds) { bounds = new Bounds(point, Vector3.zero); hasBounds = true; }
                    else bounds.Encapsulate(point);
                }
            }
            if (hasBounds)
            {
                float scale = Mathf.Min(3.5f / Mathf.Max(bounds.size.x, 0.01f), 2.8f / Mathf.Max(bounds.size.y, 0.01f));
                Vector3 center = bounds.center - model.transform.position;
                model.transform.localScale *= scale;
                model.transform.localPosition = -center * scale;
            }
            if (flipped) Flip(model.transform);
            var label = new GameObject("Name").AddComponent<TextMesh>();
            label.transform.SetParent(cell, false);
            label.transform.localPosition = new Vector3(0, -1.65f, -1);
            int separator = entry.prefab.name.IndexOf('_');
            label.text = separator < 0 ? entry.prefab.name : entry.prefab.name.Substring(0, separator)
                + "\n" + entry.prefab.name.Substring(separator + 1);
            label.anchor = TextAnchor.UpperCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 48;
            label.characterSize = 0.065f;
            label.color = new Color(0.87f, 0.92f, 1);
            label.GetComponent<MeshRenderer>().sortingOrder = 1000;
        }
        previewRoot.gameObject.SetActive(true);
        ApplyMotion();
        int rows = Mathf.Max(1, Mathf.CeilToInt(count / (float)columns));
        float width = Mathf.Min(columns, Mathf.Max(1, count)) * 5;
        float height = rows * 4.5f;
        home = new Vector3((Mathf.Min(columns, Mathf.Max(1, count)) - 1) * 2.5f, -(rows - 1) * 2.25f - 0.35f, -20);
        // Leave room for the toolbar and footer in both landscape and portrait views.
        homeSize = Mathf.Max(height * 0.5f + 1, (width * 0.5f + 1) / Mathf.Max(view.aspect, 0.1f)) * 1.25f;
        ResetView();
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }

    private void ApplyMotion()
    {
        foreach (var animator in animators)
        {
            if (animator == null || animator.runtimeAnimatorController == null) continue;
            animator.speed = paused ? 0 : speed;
            int state = Animator.StringToHash(Motions[motion]);
            if (animator.HasState(0, state)) animator.Play(state, 0, 0);
        }
    }

    private static void Flip(Transform model)
    {
        var scale = model.localScale;
        scale.x *= -1;
        model.localScale = scale;
        var position = model.localPosition;
        position.x *= -1;
        model.localPosition = position;
    }

    private void ResetView()
    {
        view.transform.position = home;
        view.orthographicSize = homeSize;
    }

    private void OnDestroy()
    {
        if (previewRoot != null) Destroy(previewRoot.gameObject);
    }

    private void Update()
    {
        if (view == null) return;
        if (screenWidth != Screen.width || screenHeight != Screen.height) Rebuild();
        if (Input.GetKeyDown(KeyCode.Home)) ResetView();
        if (Input.mousePosition.y < Screen.height - 140 && Input.mousePosition.y > 45)
            view.orthographicSize = Mathf.Clamp(view.orthographicSize * Mathf.Pow(0.85f, Input.mouseScrollDelta.y), 1, 150);
        if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1)) dragStart = Input.mousePosition;
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - dragStart;
            view.transform.position -= delta * (2 * view.orthographicSize / Screen.height);
            dragStart = Input.mousePosition;
        }
        // Replay one-shot attacks for inspection, without firing combat animation events.
        foreach (var animator in animators)
        {
            if (animator == null || animator.runtimeAnimatorController == null) continue;
            int state = Animator.StringToHash(Motions[motion]);
            if (!animator.HasState(0, state)) continue;
            var current = animator.GetCurrentAnimatorStateInfo(0);
            if (current.shortNameHash != state || !current.loop && current.normalizedTime >= 1)
                animator.Play(state, 0, 0);
        }
    }

    private void OnGUI()
    {
        if (view == null) return;
        float uiScale = Mathf.Clamp(Screen.width / 1000f, 0.6f, 1.5f);
        GUI.matrix = Matrix4x4.Scale(Vector3.one * uiScale);
        float width = Screen.width / uiScale;
        GUILayout.BeginArea(new Rect(12, 10, width - 24, 125), GUI.skin.box);
        GUILayout.Label("CHARACTER GALLERY   " + filtered.Count + " / " + characters.Length);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Search", GUILayout.Width(48));
        string nextSearch = GUILayout.TextField(search, GUILayout.MinWidth(120));
        int nextCategory = GUILayout.Toolbar(category, Categories);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        int nextMotion = GUILayout.Toolbar(motion, Motions);
        bool nextPaused = GUILayout.Toggle(paused, "Pause", GUI.skin.button, GUILayout.Width(65));
        bool nextFlipped = GUILayout.Toggle(flipped, "Flip", GUI.skin.button, GUILayout.Width(55));
        GUILayout.Label("Speed " + speed.ToString("0.0") + "x", GUILayout.Width(76));
        float nextSpeed = GUILayout.HorizontalSlider(speed, 0.1f, 2, GUILayout.Width(100));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        bool nextOverview = GUILayout.Toggle(overview, "Show all", GUI.skin.button, GUILayout.Width(90));
        GUI.enabled = !overview && page > 0;
        if (GUILayout.Button("< Previous", GUILayout.Width(100))) { page--; Rebuild(); }
        GUI.enabled = true;
        GUILayout.Label(overview ? "Overview" : "Page " + (page + 1) + " / " + Mathf.Max(1, Mathf.CeilToInt(filtered.Count / (float)PageSize)), GUILayout.Width(100));
        GUI.enabled = !overview && (page + 1) * PageSize < filtered.Count;
        if (GUILayout.Button("Next >", GUILayout.Width(100))) { page++; Rebuild(); }
        GUI.enabled = true;
        if (GUILayout.Button("Reset view", GUILayout.Width(100))) ResetView();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        if (nextSearch != search || nextCategory != category || nextOverview != overview)
        {
            search = nextSearch; category = nextCategory; overview = nextOverview; page = 0; Rebuild();
        }
        if (nextFlipped != flipped)
        {
            flipped = nextFlipped;
            foreach (var model in models) Flip(model);
        }
        if (nextMotion != motion || nextPaused != paused || !Mathf.Approximately(nextSpeed, speed))
        {
            motion = nextMotion; paused = nextPaused; speed = nextSpeed; ApplyMotion();
        }
        GUI.Label(new Rect(16, Screen.height / uiScale - 32, width - 32, 26),
            filtered.Count == 0 ? "No matching characters." : "Wheel: zoom   |   Right / middle drag: pan   |   Home: reset   |   Missing animation states keep the default pose.");
    }
}
