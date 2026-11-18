using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CharacterGalleryEditor
{
    private const string ScenePath = "Assets/01_Scenes/CharacterGallery.unity";

    [MenuItem("SceneMove/Scene_CharacterGallery")]
    private static void Open()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(ScenePath);
    }

    [MenuItem("Tools/Character Gallery/Refresh character list")]
    private static void Refresh()
    {
        var gallery = UnityEngine.Object.FindObjectOfType<CharacterGallery>();
        if (gallery == null || gallery.gameObject.scene.path != ScenePath || EditorApplication.isPlaying)
        {
            Debug.LogWarning("Open CharacterGallery.unity outside Play mode before refreshing.");
            return;
        }
        var entries = new List<CharacterGallery.Entry>();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] {
            "Assets/12_AddressableResources/Prefabs/Unit", "Assets/03_Prefabs/InGame", "Assets/Test_Easy" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            bool unit = path.StartsWith("Assets/12_AddressableResources/Prefabs/Unit/", StringComparison.Ordinal);
            bool extra = path.Contains("/Character/") || name.StartsWith("Work_Skeleton", StringComparison.Ordinal)
                || name.StartsWith("Skeleton_Gift", StringComparison.Ordinal) || path == "Assets/Test_Easy/Char_01.prefab";
            if ((!unit && !extra) || name.StartsWith("HP", StringComparison.Ordinal)) continue;
            string category = !unit ? "Extras" : path.Contains("_Object/") || name.Contains("Castle")
                ? "Objects" : path.Contains("/Hero/") ? "Heroes" : "Enemies";
            entries.Add(new CharacterGallery.Entry { prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path), category = category });
        }
        Undo.RecordObject(gallery, "Refresh character gallery");
        var order = new[] { "Heroes", "Enemies", "Objects", "Extras" };
        gallery.characters = entries.OrderBy(e => Array.IndexOf(order, e.category)).ThenBy(e => e.prefab.name, StringComparer.Ordinal).ToArray();
        EditorUtility.SetDirty(gallery);
        EditorSceneManager.MarkSceneDirty(gallery.gameObject.scene);
        Debug.Log("Character gallery refreshed: " + gallery.characters.Length + " entries. Save the scene to keep this list.");
    }
}
