using System;

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CustomEditor(typeof(PrefabPlacer))]
class PrefabPlacerEditor : Editor
{
    private PrefabPlacer p => ((PrefabPlacer)target);
    public override void OnInspectorGUI()
    {
        
        bool bttn = p.objectLists.Count > 0;
        if (bttn)
            p.gameObject.name = $"{p.current.ListName}Placer";
        if (bttn && GUILayout.Button($"Place {p.current.ListName}"))
        {
            p.PlaceObjects();
        }

        if (GUILayout.Button(($"Toggle colliders")))
        {
            p.ToggleColliders();
        }
        base.OnInspectorGUI();
    }

    protected virtual void OnSceneGUI()
    {
        if (Event.current.type != EventType.Repaint) return;
        Vector3 pos = p.transform.position;
        Handles.color = Color.green;
        Handles.CircleHandleCap(0, pos, p.transform.rotation * Quaternion.LookRotation(p.transform.up), p.SpawnRadius, EventType.Repaint);
    }
}
public class PrefabPlacer : MonoBehaviour
{
    [Header("Generates based on the first TerrainList in the List of Lists")]
    public List<TerrainGridList> objectLists = new List<TerrainGridList>();

    [HideInInspector]
    public TerrainGridList current => objectLists.First();
    public float SpawnRadius = 5f;
    public int NumberToSpawn = 1;
    public bool ClearOnSpawn = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleColliders()
    {
        var cols = GetComponentsInChildren<Collider>();
        foreach (var col in cols)
            col.enabled = !col.enabled;
    }

    private Vector3 down;
    public void PlaceObjects()
    {
        if (ClearOnSpawn)
            transform.DestroyChildrenImmediate();
        Vector3 pos = transform.position;
        pos.y += 10f;
        down = transform.up;
        down.y *= -1;
        for (int i = 0; i < NumberToSpawn; i++)
        {
            _SpawnObject(pos);
        }
    }

    void _SpawnObject(Vector3 pos)
    {
        
        pos.x += Random.Range((SpawnRadius * -1), SpawnRadius);
        pos.z += Random.Range((SpawnRadius * -1), SpawnRadius);
        RaycastHit hit;
        if (Physics.Raycast(pos, Vector3.down, out hit))
        {
            if (hit.collider.name.ToLower().Contains("waterblock"))
                return;
            if (hit.transform.root.name == "Environment")
                return;
                
            GameObject spawned = Instantiate(current.terrainSquares.GetRandom(), transform);
            spawned.name = spawned.name.Replace("(Clone)", "");
            spawned.transform.position = hit.point;
            Vector3 rot = spawned.transform.localEulerAngles;
            rot.y = Random.Range(0f, 360f);

        }
    }
}

#endif