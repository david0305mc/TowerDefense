#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;


public class SceneChangeEditor : Editor
{
    // MenuItem의 경로와 ditorSceneManager.OpenScene의 경로는 자신의 Scene 파일 이름으로 변경한다.
    [MenuItem("SceneMove/Scene_Intro &1")]
    private static void IntroScene()
    {
        EditorSceneManager.OpenScene("Assets/01_Scenes/Intro.unity");
        Debug.Log("Move Intro Scene");
    }

    [MenuItem("SceneMove/Scene_Main &2")]
    private static void MainScene()
    {
        EditorSceneManager.OpenScene("Assets/01_Scenes/Main.unity");
        Debug.Log("Move Main Scene");
    }

    [MenuItem("SceneMove/Scene_UnitTest &3")]
    private static void UnitTestScene()
    {
        EditorSceneManager.OpenScene("Assets/01_Scenes/UnitTest.unity");
        Debug.Log("Move Unit Test");
    }    
    
    [MenuItem("SceneMove/Scene_WorldMapTest &4")]
    private static void WorldMapTestScene()
    {
        EditorSceneManager.OpenScene("Assets/01_Scenes/WorldMapTest.unity");
        Debug.Log("Move WorldMapTest Test");
    }



}
#endif