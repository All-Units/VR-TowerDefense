using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class TowerDatabaseView : EditorWindow
{
    public List<Tower_SO> towerSos = new();
    
    public static void ShowWindow()
    {
        var window = GetWindow<TowerDatabaseView>("Tower Database");
        
        string[] guids = AssetDatabase.FindAssets("t:Tower_SO");
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Tower_SO towerSo = AssetDatabase.LoadAssetAtPath<Tower_SO>(assetPath);
            
            if(towerSo)
                window.towerSos.Add(towerSo);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("List of Game Levels:", EditorStyles.boldLabel);

        foreach (var towerSo in towerSos)
        {
            
        }
    }
}
