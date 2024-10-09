using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

#if UNITY_EDITOR


[CustomEditor(typeof(StatTracker), true), CanEditMultipleObjects]
public class StatTrackerEditor : Editor
{
    private StatTracker s => ((StatTracker)target);
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Zero Stat"))
        {
            foreach (var obj in Selection.objects)
            {
                if (obj != null && obj is StatTracker stat)
                {
                    stat.ResetTotal();
                    Debug.Log($"Zeroing: {stat.name}"); }
            }

        }
        if (GUILayout.Button("Force Serialize"))
        {
            foreach (var obj in Selection.objects)
            {
                if (obj != null && obj is StatTracker stat)
                {
                    stat.Serialize();
                    //Debug.Log($"Serializing: {stat.name}");
                }
            }
        }
        if (GUILayout.Button("Force Deserialize"))
        {
            foreach (var obj in Selection.objects)
            {
                if (obj != null && obj is StatTracker stat)
                {
                    stat.Deserialize();
                    //Debug.Log($"Serializing: {stat.name}");
                }
            }
        }
        bool _NoSpaces = true;
        foreach (var obj in Selection.objects)
            if (obj.name.Contains(" ")) _NoSpaces = false;
        //If any selected obj has a space
        if (_NoSpaces == false && GUILayout.Button("Remove Spaces"))
        {
            foreach (var obj in Selection.objects)
            {
                if (obj != null && obj is StatTracker stat)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    string noSpaces = obj.name.Replace(" ", "");
                    AssetDatabase.RenameAsset(path, noSpaces);
                    obj.name = noSpaces;
                    stat.name = noSpaces;
                }
            }
        }




        /*
        bool bttn = s.prefabs;

        if (bttn)

        {
            Vector3 pos = s.gameObject.transform.position;
            s.gameObject.name = $"{s.prefabs.name} ({pos.x}, {pos.z})";
        }
        if (bttn && GUILayout.Button($"Place {s.prefabs.name}"))
        {
            s.PlaceObjects();
            foreach (var go in Selection.objects)
            {
                var placer = ((GameObject)go).GetComponent<ScriptablePrefabPlacer>();
                if (placer && placer != s)
                {
                    placer.PlaceObjects();
                }
            }
        }
        if (bttn && GUILayout.Button("Add all gameobjects in folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Folder", $"{Application.dataPath}/Imported/LMHPOLY/Low Poly Nature Bundle", "");
            s.AddAllGOsInFolder(path);
        }

        if (GUILayout.Button(($"Toggle colliders")))
        {
            s.ToggleColliders();
        }*/
        base.OnInspectorGUI();

        if (BaseStats == null) return;
        bool _isInBaseStats = false;
        foreach (var stat in _SelectedStats())
            _isInBaseStats = _isInBaseStats || BaseStats.trackers.Contains(stat);
        if (_isInBaseStats == false && GUILayout.Button($"Add to Base Stats"))
        {
            foreach (var obj in Selection.objects)
            {
                if (obj != null && obj is StatTracker stat)
                    BaseStats.trackers.Add(stat);
            }
        }

    }
    List<StatTracker> _SelectedStats()
    {
        List<StatTracker> _selected = new List<StatTracker>();
        foreach (var obj in Selection.objects)
        {
            if (obj != null && obj is StatTracker stat)
                _selected.Add(stat);
        }
        return _selected;
    }
    StatTrackerHolder BaseStats { 
        get 
        {
            if (_baseStats == null)
                _baseStats = Resources.Load<StatTrackerHolder>("Base Stats");
            return _baseStats;
        } 
        }
    StatTrackerHolder _baseStats;


    protected virtual void OnSceneGUI()
    {
        /*
        if (Event.current.type != EventType.Repaint) return;
        Vector3 pos = s.transform.position;
        Handles.color = Color.green;
        Handles.RectangleHandleCap(0, pos, s.transform.rotation * Quaternion.LookRotation(s.transform.up), s.SpawnRadius, EventType.Repaint);
        */
    }
}

[CanEditMultipleObjects]
#endif

public abstract class StatTracker : ScriptableObject
{
    public string key;
    public string displayName;
    public string statName;

    public int total = 0;
    [SerializeField] private bool _isInitialized = false;
    public Color displayTextColor = Color.clear;

    public void Initialize(bool _force = false)
    {
        if (_force)
            _isInitialized = false;
        if (_isInitialized) return;

        InitTracker();
        _isInitialized = true;
    }

    protected abstract void InitTracker();
    public abstract void ClearTracker();

    public virtual void ResetTotal()
    {
        total = 0;
        SerializeIfChanged();
    }

    public int getSerializeValue => PlayerPrefs.GetInt(key, 0);
    public void SerializeIfChanged()
    {
        if (total == getSerializeValue) return;
        Serialize();
    }
    
    public virtual void Serialize()
    {
        PlayerPrefs.SetInt(key, total);
    }
    
    public virtual void Deserialize()
    {
        total = getSerializeValue;
    }

    public abstract void Print();
}