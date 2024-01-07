#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ScriptablePrefabPlacer))]
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
        Handles.RectangleHandleCap(0, pos, p.transform.rotation * Quaternion.LookRotation(p.transform.up), p.SpawnRadius, EventType.Repaint);
    }
}
[CanEditMultipleObjects]
public class ScriptablePrefabPlacer : MonoBehaviour
{
    public PrefabList_SO prefabs;
    public float SpawnRadius = 5f;
    public int NumberToSpawn = 1;
    public bool ClearOnSpawn = true;
    public bool IsMountains;

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
        if (Physics.Raycast(pos, Vector3.down, out hit))
        {
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
}
#endif