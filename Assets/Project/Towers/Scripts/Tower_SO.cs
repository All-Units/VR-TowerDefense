using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Tower", fileName = "New Tower")]
[Serializable]
public class Tower_SO : ScriptableObject
{
    public bool IsUnlocked = true;
    [Header("Prefabs")]
    public GameObject ghostObject;
    public Tower towerPrefab;
    public GameObject iconPrefab;
    public GameObject minimapPrefab;
    
    [Header("Base Data")]
    public new string name;
    public string description;
    public int cost;
    public int maxHeath;

    [SerializeField] private List<TowerUpgrade> upgrades = new();

    public List<TowerUpgrade> GetUpgrades() => upgrades;
    
#if UNITY_EDITOR
    [ContextMenu("Create Destroyed and Placed Trackers")]
    private void CreateTrackers()
    {
        // Create new instances of TowerDestroyedTracker and TowerPlacedTracker
        var destroyedTracker = CreateInstance<TowerDestroyedTracker>();
        var placedTracker = CreateInstance<TowerPlacedTracker>();

        // Set the names for the new instances
        var towerName = name;

        // Set the name for the destroyed tracker
        destroyedTracker.name = towerName + " Lost";
        placedTracker.name = towerName + " Placed";
        
        // Assign the Tower_SO to the _towerToTrack field of each new scriptable object
        destroyedTracker._towerToTrack = this;
        placedTracker._towerToTrack = this; 
        
        // Assign the Tower_SO to the _towerToTrack field of each new scriptable object
        destroyedTracker.statName = "Lost";
        placedTracker.statName = "Placed";
        
        destroyedTracker.key = towerName + "Lost";
        placedTracker.key = towerName + "Placed";

        // Save the new instances as assets
        var destroyedTrackerPath = k_TrackerPath + destroyedTracker.name + ".asset";
        AssetDatabase.CreateAsset(destroyedTracker, destroyedTrackerPath);
        var placedTrackerPath = k_TrackerPath + placedTracker.name + ".asset";
        AssetDatabase.CreateAsset(placedTracker, placedTrackerPath);
        
        // Refresh the asset database to reflect the changes
        AssetDatabase.Refresh();
    }    
    
    [ContextMenu("Create Kill Tracker")]
    private void CreateKillTracker()
    {
        // Create new instances of TowerDestroyedTracker and TowerPlacedTracker
        var towerKillsTracker = CreateInstance<TowerKillsTracker>();

        // Set the names for the new instances
        var towerName = name;

        // Set the name for the destroyed tracker
        towerKillsTracker.name = towerName + " Kills";
        
        // Assign the Tower_SO to the _towerToTrack field of each new scriptable object
        towerKillsTracker._towerToTrack = this;
        
        // Assign the Tower_SO to the _towerToTrack field of each new scriptable object
        towerKillsTracker.statName = "Kills";
        
        towerKillsTracker.key = towerName + "Kills";

        // Save the new instances as assets
        var destroyedTrackerPath = k_TrackerPath + towerKillsTracker.name + ".asset";
        AssetDatabase.CreateAsset(towerKillsTracker, destroyedTrackerPath);
        
        var displayPath = k_AssetsProjectStatsSoDisplaysTowers + towerName + " Display" + ".asset";
        var display = AssetDatabase.LoadAssetAtPath<StatDisplayModel>(displayPath);
        if(display == null)
        {
            CreateStatDisplay();
            display = AssetDatabase.LoadAssetAtPath<StatDisplayModel>(displayPath);
        }      
        
        Undo.RecordObject(display, "Set Tracker");
        display.statTrackers.Add(towerKillsTracker);
        EditorUtility.SetDirty(display);
        
        // Refresh the asset database to reflect the changes
        AssetDatabase.Refresh();
    }
    
    [ContextMenu("Create Stat Display")]
    private void CreateStatDisplay()
    {
        var towerName = name;
        
        var destroyedTrackerPath = k_TrackerPath + towerName + " Lost" + ".asset";
        var placedTrackerPath = k_TrackerPath + towerName + " Placed" + ".asset";
        
        var towerDisplay = CreateInstance<StatDisplayModel>();
        towerDisplay.name = towerName + " Display";
        towerDisplay.displayName = towerName;
        towerDisplay.statTrackers.Add(AssetDatabase.LoadAssetAtPath<TowerDestroyedTracker>(destroyedTrackerPath));
        towerDisplay.statTrackers.Add(AssetDatabase.LoadAssetAtPath<TowerPlacedTracker>(placedTrackerPath));
        
        var displayPath = k_AssetsProjectStatsSoDisplaysTowers + towerDisplay.name + ".asset";
        AssetDatabase.CreateAsset(towerDisplay, displayPath);

        // Refresh the asset database to reflect the changes
        AssetDatabase.Refresh();
    }
#endif

    private const string k_AssetsProjectStatsSoDisplaysTowers = "Assets/Project/Stats/SO/Displays/Towers/";
    private const string k_TrackerPath = "Assets/Project/Stats/SO/Trackers/Towers/";
}

[Serializable]
public struct TowerUpgrade
{
    public Tower_SO upgrade;
    public int cost;
}