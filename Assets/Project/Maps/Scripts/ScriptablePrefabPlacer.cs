using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;

using Application = UnityEngine.Application;

[CustomEditor(typeof(ScriptablePrefabPlacer))]
[CanEditMultipleObjects]
class ScriptablePrefabPlacerEditor : Editor
{
    private ScriptablePrefabPlacer p => ((ScriptablePrefabPlacer)target);
    public override void OnInspectorGUI()
    {
        bool bttn = p.prefabs;
        
        if (bttn)

        {
            Vector3 pos = p.gameObject.transform.position; 
            p.gameObject.name = $"{p.prefabs.name} ({pos.x}, {pos.z})"; 
        }
        if (bttn && GUILayout.Button($"Place {p.prefabs.name}"))
        {
            p.PlaceObjects();
            foreach (var go in Selection.objects)
            {
                var placer = ((GameObject)go).GetComponent<ScriptablePrefabPlacer>();
                if (placer && placer != p)
                {
                    placer.PlaceObjects();
                }
            }
        }
        if (bttn && GUILayout.Button("Add all gameobjects in folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Folder", $"{Application.dataPath}/Imported/LMHPOLY/Low Poly Nature Bundle", "");
            p.AddAllGOsInFolder(path);
        }

        if (GUILayout.Button(($"Toggle colliders")))
        {
            p.ToggleColliders();
        }
        if (bttn && GUILayout.Button("Clear Children"))
        {
            for (int i = p.transform.childCount -1; i >= 0; i--)
            {
                DestroyImmediate(p.transform.GetChild(i).gameObject);
            }
            foreach (Transform t in p.transform)
            {
                DestroyImmediate(t.gameObject);
            }
            EditorUtility.SetDirty(p);
        }
        if (bttn && GUILayout.Button("Destroy LODs & Colliders"))
        {
            
            foreach (Transform child in p.transform.GetAllDescendants())
            {
                //Transform child = p.transform.GetChild(i);
                LODGroup lod = child.GetComponent<LODGroup>();
                if (lod) DestroyImmediate(lod);
                Collider col = child.GetComponent<Collider>();
                if (col != null) { DestroyImmediate(col); }
                EditorUtility.SetDirty(child);
                //DestroyImmediate(p.transform.GetChild(i).gameObject);
            }
            
            //EditorUtility.SetDirty(p);
        }
        base.OnInspectorGUI();
    }

    protected virtual void OnSceneGUI()
    {
        if (Event.current.type != EventType.Repaint) return;
        Vector3 pos = p.transform.position;
        Handles.color = Color.green;
        Handles.RectangleHandleCap(0, pos, p.transform.rotation * Quaternion.LookRotation(p.transform.up), p.SpawnRadius, EventType.Repaint);
    }
}

[CanEditMultipleObjects]
#endif
public class ScriptablePrefabPlacer : MonoBehaviour
{
    public PrefabList_SO prefabs;
    [Range(1f, 900f)]
    public float SpawnRadius = 50f;
    public int NumberToSpawn = 75;
    public bool ClearOnSpawn = true;
    public bool IsMountains;
#if UNITY_EDITOR
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
        pos.y += 1000f;
        down = transform.up;
        down.y *= -1;
        for (int i = 0; i < NumberToSpawn; i++)
        {
            _SpawnObject(pos);
        }
    }
    bool _Blacklist(RaycastHit hit)
    {
        string name = hit.collider.name.ToLower();
        string[] blacklist = new string[] { "water", "walkway", "wall"};
        if (hit.collider.gameObject.layer != 7)
            return false;
        foreach (string black in blacklist)
        {
            if (name.Contains(black))
                return true;
        }
        var parent = hit.collider.transform;
        while (parent != null)
        {
            
            name = parent.gameObject.name.ToLower();
            if (name.Contains("castle"))
                return true;
            parent = parent.parent;

        }
        
        return false;
    }

    void _SpawnObject(Vector3 pos)
    {
        
        pos.x += Random.Range((SpawnRadius * -1), SpawnRadius);
        pos.z += Random.Range((SpawnRadius * -1), SpawnRadius);
        
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Ground");
        if (Physics.Raycast(pos, Vector3.down, out hit, float.PositiveInfinity, mask))
        {
            if (_Blacklist(hit)) print($"Skipping bc blacklist");
            if (_Blacklist(hit) && IsMountains == false)
                return;


            //GameObject spawned = Instantiate(prefabs.GetRandom(), transform);
            GameObject spawned = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.GetRandom(), transform); 
            spawned.transform.localScale = Vector3.one * Random.Range(prefabs.PrefabScaleBounds.x, prefabs.PrefabScaleBounds.y);
            spawned.name = spawned.name.Replace("(Clone)", "");
            Vector3 point = hit.point;
            if (IsMountains)
                point.y = 0f;
            spawned.transform.position = point;
            
            spawned.layer = 7;
            foreach (Transform c in spawned.transform.GetAllDescendants())
                c.gameObject.layer = 7;
            Vector3 rot = spawned.transform.localEulerAngles;
            rot.y = Random.Range(0f, 360f);
            spawned.transform.eulerAngles = rot;

            if (prefabs.texture != null)
            {
                foreach (MeshRenderer mr in spawned.GetComponentsInChildren<MeshRenderer>())
                    mr.material = prefabs.texture;
            }

        }
    }

    public void AddAllGOsInFolder(string path)
    {
        if (path == null || path == "") return;
        string[] files = Directory.GetFiles(path, "*.prefab");
        List<GameObject> prefabs = new List<GameObject>();
        string shortPath = path.Replace(Application.dataPath, "Assets/");
        foreach (var filePath in files)
        {
            string p = filePath.Replace(Application.dataPath, "Assets/");
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (go == null) continue;
            print($"Adding {go.name} to list");
            prefabs.Add(go);

        }
        this.prefabs.prefabs.AddRange(prefabs);
        print($"Selected folder {shortPath}. It had {prefabs.Count} objs");
        EditorUtility.SetDirty(this.prefabs);

    }
#endif
}
