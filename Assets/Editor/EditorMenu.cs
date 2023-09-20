using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorMenu 
{
    [MenuItem("Tools/GenerateTableCode")]
    public static void GenerateTableCode()
    {
        if (EditorApplication.isPlaying)
        {
            return;
        }
        
        DataManager.GenDatatable();
        DataManager.GenConfigTable();
        DataManager.GenTableEnum();
        Debug.Log("GenerateTableCode");
    }

    [MenuItem("Tools/ClearUserData")]
    public static void ClearUserData()
    {
        if (EditorApplication.isPlaying)
        {
            return;
        }
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath);
        foreach (string filePath in filePaths)
            File.Delete(filePath);
    }
}
