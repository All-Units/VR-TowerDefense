using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR


[CustomEditor(typeof(ScriptableTerrainGridGenerator))]
class ScriptableTerrainGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Generate grid"))
        {
            var t = (ScriptableTerrainGridGenerator)target;
            t.GenerateGrid();
        }
        if (GUILayout.Button("Add water"))
        {
            var t = (ScriptableTerrainGridGenerator)target;
            t.AddAllWater();
        }
        if (GUILayout.Button("Set all as ground"))
        {
            var t = (ScriptableTerrainGridGenerator)target;
            t.SetAllAsGround(t.transform);
        }
        if (GUILayout.Button("Clear grid"))
        {
            var t = (ScriptableTerrainGridGenerator)target;
            t.ClearChildren();
        }
        base.OnInspectorGUI();
    }
}
#endif
public class ScriptableTerrainGridGenerator : MonoBehaviour
{
    public int size = 7;
    public float scale = 1.0001f;
    public GameObject waterPrefab;
    public float WaterLevel = -1.3f;

    public TerrainGridList_SO terrainList;
    public bool clearPrevious = false;

    List<Vector3> vertices = new List<Vector3>();
    Dictionary<Vector3, Vector3> _ToFix = new Dictionary<Vector3, Vector3>();
    private void Awake()
    {
        foreach (var mf in GetComponentsInChildren<MeshFilter>())
        {
            _SmoothAllCorners(mf);
        }
    }
    bool _IsRiver(MeshFilter mf)
    {
        var name = mf.gameObject.name.ToLower();
        bool isRiver = name.Contains("river");
        if (isRiver)
        {
            _SpawnWater(mf);
        }
        return isRiver;
    }
    void _SpawnWater(MeshFilter mf)
    {
        Transform offset = mf.transform.parent.parent;
        //We don't have a water prefab yet
        if (offset.Find("water") == null)
        {
#if UNITY_EDITOR
            GameObject water = (GameObject)PrefabUtility.InstantiatePrefab(waterPrefab); //Instantiate(waterPrefab);
#else
GameObject water = Instantiate(waterPrefab);
#endif
            water.name = "water";
            water.transform.localScale = Vector3.one * 2f;
            water.transform.eulerAngles = Vector3.zero;
            water.transform.parent = offset;
            water.transform.localPosition = new Vector3(50f, WaterLevel, 50f);
        }
        else
        {
            var water = offset.Find("water");
            water.parent = null;
            water.transform.localScale = Vector3.one * 2f;
            water.transform.eulerAngles = Vector3.zero;
            water.transform.parent = offset;
            water.transform.localPosition = new Vector3(50f, WaterLevel, 50f);
        }

    }
    void _SmoothAllCorners(MeshFilter mf)
    {
        if (mf.gameObject.name.StartsWith("water")) return;
        List<Vector3> vertices = new List<Vector3>();
        foreach (var vert in mf.sharedMesh.vertices)
        {
            var v = vert;
            
            if (v.x <= 0.9) v.x = 0f;
            if (v.z <= 0.9) v.z = 0f;
            if (v.x >= 99.1f) v.x = 100f;
            if (v.z >= 99.1f) v.z = 100f;
            if (v != vert)
            {
                //Only round close values on rivers
                if (_IsRiver(mf) && MathF.Abs(v.y) <= 0.1f)
                    v.y = 0f;
                else if (_IsRiver(mf) == false)
                    v.y = 0f;
            }
            
            
            vertices.Add(v);
        }
        mf.mesh.vertices = vertices.ToArray();
        mf.mesh.RecalculateBounds();
    }
#if UNITY_EDITOR
    public void GenerateGrid()
    {
        if (clearPrevious)
            ClearChildren();
        
        for (int i = -size; i < size; i++)
        {
            for (int j = -size; j < size; j++)
            {
                var prefab = terrainList.terrainSquares.GetRandom();
                Vector2 pos = new Vector2(i, j);
                _SpawnGridAt(pos, prefab);
            }
        }
        SetAllAsGround(transform);
        
        
    }
    public void AddAllWater()
    {
        foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
        {
            _IsRiver(mf);
        }    
    }
    public void SmoothAllCorners()
    {
        _ToFix.Clear();
        print("Smoothing corners!");
        MeshFilter mf = transform.GetChild(0).GetComponentInChildren<MeshFilter>();
        _SmoothTileCorners(-7, -7);
        //var vertices = GetCornerVerts(mf);
        //_lastNeigbors = _GetNeighbors(-4, -4);
    }
    Vector3 lastPos;
    List<MeshFilter> _lastNeigbors = new List<MeshFilter>();
    void _SmoothTileCorners(int i, int j)
    {
        MeshFilter tile = _GetTile(i, j);
        var verts = GetCornerVerts(tile);
        Dictionary<MeshFilter, List<Vector3>> neighbors = new Dictionary<MeshFilter, List<Vector3>>();
        foreach (var neighbor in _GetNeighbors(i, j))
        {
            neighbors.Add(neighbor, GetCornerVerts(neighbor));
        }
        //Iterate over all corner verts
        foreach (var vert in verts)
        {
            //Find the closest neighbor vertex to ourselves
            var neighbor = _FindClosestNeighborVertex(ref neighbors, vert);
            if (neighbor.Key != null)
                _ToFix[vert] = neighbor.Value;
        }
    }
    KeyValuePair<MeshFilter, Vector3> _FindClosestNeighborVertex(ref Dictionary<MeshFilter, List<Vector3>> neighbors, Vector3 vert)
    {
        KeyValuePair<MeshFilter, Vector3> closest = new KeyValuePair<MeshFilter, Vector3>();
        Vector3 current = Vector3.positiveInfinity;
        foreach (MeshFilter mesh in neighbors.Keys)
        {
            foreach (var v in neighbors[mesh])
            {
                if (vert.FlatDistance(v) < vert.FlatDistance(current) && vert.FlatDistance(v) < 2f)
                {
                    current = v;
                    closest = new KeyValuePair<MeshFilter, Vector3> (mesh, current );
                }
            }
        }

        return closest;
    }
    List<Vector3> GetCornerVerts(MeshFilter mf, bool world = true)
    {
        var verts = mf.sharedMesh.vertices;
        verts = verts.OrderBy(x => x.x).ToArray();
        Vector3 p = mf.transform.position;
        lastPos = p;
        var vertices = new List<Vector3>();
        foreach (var v in verts)
        {
            if (v.x <= 0.1f || v.x >= 99f || v.z <= 0.1f || v.z >= 99f)
            {
                if (world)
                    vertices.Add(mf.transform.TransformPoint(v));
                else
                    vertices.Add(v);
            }

        }
        return vertices;
    }
    MeshFilter _GetTile(int i, int j)
    {
        string name = $"{i}, {j}";
        //Neighbor exists

        Transform t = transform.Find(name);
        if (t != null)
        {
            var mfs = t.GetComponentsInChildren<MeshFilter>().ToList();
            var mesh = mfs.Find(x => x.gameObject.name.Contains("LOD0"));
            if (mesh != null) return mesh;
        }
        return null;
    }
    List<MeshFilter> _GetNeighbors(int i, int j)
    {
        List<MeshFilter> neighbors = new List<MeshFilter>();
        for (int a  = -1; a < 2; a++)
        {
            for (int b = -1; b < 2; b++)
            {
                if (a == 0 && b == 0) continue;
                string name = $"{i + a}, {j + b}";
                //Neighbor exists

                Transform t = transform.Find(name);
                if (t != null)
                {
                    var mfs = t.GetComponentsInChildren<MeshFilter>().ToList();
                    var mesh = mfs.Find(x => x.gameObject.name.Contains("LOD0"));
                    if (mesh != null) neighbors.Add(mesh);
                }
            }
        }

        return neighbors;
    }

    void _SpawnGridAt(Vector2 pos, GameObject prefab)
    {
        Transform parent = new GameObject().transform;
        var directions = new[] { 0, 90, 180, 270 }.ToList();
        
        parent.name = $"{(int)pos.x}, {(int)pos.y}";
        parent.parent = transform;
        Transform offset = new GameObject().transform;
        offset.gameObject.name = "Offset";
        offset.parent = parent;
        offset.localPosition = new Vector3(-50f, 0f, -50f);
        GameObject spawned = (GameObject)PrefabUtility.InstantiatePrefab(prefab, offset); //Instantiate(prefab, offset);
        spawned.name = spawned.name.Replace("(Clone)", "");
        parent.localPosition = new Vector3(pos.x * 100, 0f, pos.y * 100);
        spawned.transform.localScale *= scale;
        parent.localEulerAngles = new Vector3(0, directions.GetRandom(), 0);

        foreach (MeshRenderer mr in spawned.GetComponentsInChildren<MeshRenderer>(true))
        {
            mr.material = terrainList.texture;
        }

    }

    public void ClearChildren()
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
    }

    public void SetAllAsGround(Transform t)
    {
        int childCount = t.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = t.GetChild(i);
            if (child.name != "water")
                child.gameObject.layer = 7;
            SetAllAsGround(child);
        }
    }
    
#endif
}