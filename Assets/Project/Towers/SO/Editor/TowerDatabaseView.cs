using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TowerDatabaseView : EditorWindow
{
    private List<Tower_SO> _towerSos = new();
    
    public static void ShowWindow()
    {
        var window = GetWindow<TowerDatabaseView>();
        window.titleContent = new GUIContent("Tower Database");
    }

    private void CreateGUI()
    {
        
    }

    private void OnGUI()
    {
        
    }
}
