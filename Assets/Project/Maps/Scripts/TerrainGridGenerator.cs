using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR


[CustomEditor(typeof(TerrainGridGenerator))]
class TerrainGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Generate grid"))
        {
            var t = (TerrainGridGenerator)target;
            t.GenerateGrid();
        }
        if (GUILayout.Button("Clear grid"))
        {
            var t = (TerrainGridGenerator)target;
            t.ClearChildren();
        }
        base.OnInspectorGUI();
    }
}
public class TerrainGridGenerator : MonoBehaviour
{
    public int size = 7;

    public List<TerrainGridList> terrains = new List<TerrainGridList>();
    public bool clearPrevious = false;
    
    
    public void GenerateGrid()
    {
        if (clearPrevious)
            ClearChildren();
        var terrain = terrains.GetRandom();
        
        for (int i = -3; i < size; i++)
        {
            
            for (int j = -3; j < size; j++)
            {
                if (i >= 0 && j >= 0 && i < 9 && j < 9)
                    continue;
                var prefab = terrain.terrainSquares.GetRandom();
                Vector2 pos = new Vector2(i, j);
                _SpawnGridAt(pos, prefab);
            }
        }
        SetAllAsGround(transform);
    }

    void _SpawnGridAt(Vector2 pos, GameObject prefab)
    {
        Transform parent = new GameObject().transform;
        parent.name = $"{(int)pos.x}, {(int)pos.y}";
        parent.parent = transform;
        Transform offset = new GameObject().transform;
        offset.gameObject.name = "Offset";
        offset.parent = parent;
        offset.localPosition = new Vector3(-50f, 0f, -50f);
        GameObject spawned = Instantiate(prefab, offset);
        parent.localPosition = new Vector3(pos.x * 100, 0f, pos.y * 100);
        //spawned.transform.localPosition = new Vector3(-50f, 0f, -50f);
    }

    public void ClearChildren()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
    }

    void SetAllAsGround(Transform t)
    {
        int childCount = t.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = t.GetChild(i);
            child.gameObject.layer = 7;
            SetAllAsGround(child);
        }
    }
}

[Serializable]
public struct TerrainGridList
{
    public string ListName;
    public List<GameObject> terrainSquares;
}
#endif