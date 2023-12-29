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
            p.gameObject.name = $"{p.prefabs.name}Placer";
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
        Handles.CircleHandleCap(0, pos, p.transform.rotation * Quaternion.LookRotation(p.transform.up), p.SpawnRadius, EventType.Repaint);
    }
}
public class ScriptablePrefabPlacer : MonoBehaviour
{
    public PrefabList_SO prefabs;
    public float SpawnRadius = 5f;
    public int NumberToSpawn = 1;
    public bool ClearOnSpawn = true;

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
            if (hit.collider.name.ToLower().Contains("water"))
                return;
            
                
            GameObject spawned = Instantiate(prefabs.GetRandom(), transform);
            spawned.name = spawned.name.Replace("(Clone)", "");
            spawned.transform.position = hit.point;
            Vector3 rot = spawned.transform.localEulerAngles;
            rot.y = Random.Range(0f, 360f);
            spawned.transform.eulerAngles = rot;

        }
    }
}
#endif