using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
public partial class Utill
{
    public static T MakeGameObject<T>(string objectname, Transform parent) where T : MonoBehaviour
    {
        return MakeGameObject(objectname, parent).AddComponent<T>();
    }
    public static GameObject MakeGameObject(string objectname, Transform parent)
    {
        GameObject TempObject = new GameObject(objectname);
        if (parent != null)
        {
            TempObject.transform.SetParent(parent);
        }
        return TempObject;
    }

    public static T InstantiateGameObject<T>(GameObject prefab, Transform parent = null) where T : MonoBehaviour
    {
        return InstantiateGameObject(prefab, parent).GetComponent<T>();
    }
    public static GameObject InstantiateGameObject(GameObject prefab, Transform parent = null)
    {
        var go = GameObject.Instantiate(prefab, parent, false);
        go.name = prefab.name;
        return go;
    }

    public static string LoadFromFile(string filePath)
    {
        string text;
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                text = sr.ReadToEnd();
            }
        }
        return text;
    }

    public static async UniTask<string> LoadFromFileAsync(string filePath)
    {
        string text;
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            using (StreamReader sr = new StreamReader(fs))
            {
                text = await sr.ReadToEndAsync();
            }
        }
        return text;
    }

    public static T Load<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path);
    }
}


public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.data;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.data = array;
        return JsonUtility.ToJson(wrapper);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] data;
    }

}



public static class PathInfo
{
    private static string _dataPath = string.Empty;
    public static string DataPath
    {
        get
        {
            if (_dataPath == string.Empty)
            {
                StringBuilder sb = new StringBuilder();

                if (Application.isEditor == true)
                {
                    sb.Append(Application.dataPath);
                    sb.Append("/../Cache");
                }
                else
                {
                    sb.Append(Application.persistentDataPath);
                }

                _dataPath = sb.ToString();
            }

            return _dataPath;
        }
    }
}