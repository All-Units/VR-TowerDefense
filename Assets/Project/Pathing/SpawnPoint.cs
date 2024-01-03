using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;


[CustomEditor(typeof(SpawnPoint))]
class SpawnPointEditor : Editor
{
    private SpawnPoint sp => (SpawnPoint)target;
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Place all roads"))
        {
            sp.PlaceRoads();
        }
        base.OnInspectorGUI();
    }
}


#endif


public class SpawnPoint : PathPoint
{
    [SerializeField] private SpawnPointData data;
    public Transform enemyParent;
    public GameObject roadTilePrefab;
    [SerializeField] private LayerMask groundLayers;
    public float roadSpacing = 1f;
    public Vector2 horizontalRoadSpacing = new Vector2(1,2);
    public SpawnPointData GetData() => data;
#if UNITY_EDITOR

    [SerializeField][HideInInspector]
    Transform _roadParent;

    PathPoint _currentTarget;
    PathPoint _current;

    [SerializeField] PathPoint _lastPoint;
    Dictionary<Vector3, Vector3> points = new Dictionary<Vector3, Vector3>();
    public void PlaceRoads()
    {
        print($"Placing all roads!");
        _currentTarget = nextPoint;
        _current = this;
        _InitRoadParent();
        
        var pos = transform.position;
        points.Clear();
        while ( _currentTarget.nextPoint != null)
        {
            pos = _current.transform.position;
            pos.y += 100f;
            var target = _currentTarget.transform.position;
            bool left = true;
            while (pos.FlatDistance(target) >= roadSpacing)
            {
                Vector3 dir = target - pos; dir.y = 0f;
                dir = dir.normalized;
                _SpawnTile(pos, left);
                left = !left;
                pos += dir * roadSpacing;
            }
            _current = _currentTarget;
            _currentTarget = _currentTarget.nextPoint;
            if (_lastPoint != null && _lastPoint == _currentTarget)
                break;

        }
    }
    void _SpawnTile(Vector3 pos, bool left = false)
    {

        //pos.x += Random.Range((SpawnRadius * -1), SpawnRadius);
        //pos.z += Random.Range((SpawnRadius * -1), SpawnRadius);
        RaycastHit hit;
        Vector3 dir = _current.transform.right.normalized;
        dir *= Random.Range(horizontalRoadSpacing.x, horizontalRoadSpacing.y);
        if (left == true)
            dir *= -1f;
        pos += dir;
        if (Physics.Raycast(pos, Vector3.down, out hit, Mathf.Infinity, groundLayers))
        {
            if (hit.collider.name.ToLower().Contains("water"))
                return;


            GameObject spawned = Instantiate(roadTilePrefab, _roadParent);
            spawned.name = spawned.name.Replace("(Clone)", "");
            spawned.transform.position = hit.point;
            Vector3 rot = spawned.transform.localEulerAngles;
            rot.y = Random.Range(0f, 360f);
            spawned.transform.eulerAngles = rot;
            points.Add(hit.point, hit.point + hit.normal);
            spawned.transform.LookAt(hit.point + hit.normal);
        }
    }
    private void OnDrawGizmosSelected()
    {
        foreach (var p in points)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(p.Key, p.Value);
        }
    }
    void _InitRoadParent()
    {
        if (_roadParent == null)
        {
            _roadParent = new GameObject().transform;
            _roadParent.parent = transform.parent;
            _roadParent.gameObject.name = "Road Tile Parent";
        }
        _roadParent.DestroyChildrenImmediate();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        var position = transform.position;
        this.position = position;
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(position, .6f);
        if (data)
        {

        }
        var next = nextPoint;
        while (next)
        {
            Gizmos.color = Color.magenta;
            //Gizmos.DrawLine(position, next.transform.position);

            position = next.transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(position, .5f);

            next = next.nextPoint;
            break;
        }
        gameObject.name = $"Spawn Point {transform.parent.name}";
    }

    private const string DataAssetPath = "Assets/Project/Scriptables/SpawnPoint";

    [MenuItem("CONTEXT/SpawnPoint/GenerateSpawnData")]
    public static void GenerateSpawnData()
    {
        foreach (SpawnPoint spawnPoint in FindObjectsOfType<SpawnPoint>())
        {
            if (spawnPoint.data != null) continue;

            SpawnPointData[] allSpawnPointData = AssetDatabase.FindAssets("t:SpawnPointData")
                .Select(guid => AssetDatabase.LoadAssetAtPath<SpawnPointData>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();

            SpawnPointData nextAvailableSpawnPointData = allSpawnPointData
                .FirstOrDefault(data => !AssetDatabase.IsMainAsset(data) && !AssetDatabase.IsSubAsset(data));

            if (nextAvailableSpawnPointData == null)
            {
                int nextSpawnPointDataNumber = allSpawnPointData.Length + 1;
                string newSpawnPointDataName = "SpawnPointData" + nextSpawnPointDataNumber.ToString();
                string newSpawnPointDataPath = DataAssetPath + "/" + newSpawnPointDataName + ".asset";
                nextAvailableSpawnPointData = ScriptableObject.CreateInstance<SpawnPointData>();
                AssetDatabase.CreateAsset(nextAvailableSpawnPointData, newSpawnPointDataPath);
                AssetDatabase.SaveAssets();
            }

            spawnPoint.data = nextAvailableSpawnPointData;
        }
    }

    [MenuItem("PathPoint/CreateNewSpawnPointFromSelected ^b"), MenuItem("GameObject/Pathing/New Connected Spawn Point")]
    public static void CreateNewSpawnPointFromSelected(MenuCommand menuCommand)
    {
        Debug.Log("Creating Path Points");
        var newPoints = new List<GameObject>();
        foreach (var selected in Selection.gameObjects)
        {
            if (selected.TryGetComponent(out PathPoint pathPoint) == false) continue;

            var newPoint = new GameObject(selected.name.IterateSuffix());
            var pp = newPoint.AddComponent<SpawnPoint>();
            pp.nextPoint = pathPoint;
            newPoint.transform.SetParent(selected.transform.parent);
            newPoint.transform.position = selected.transform.position;
            newPoints.Add(newPoint);
        }

        Selection.objects = newPoints.ToArray();
    }

    [MenuItem("GameObject/Pathing/New Spawn Point")]
    public static void CreateNewSpawnPoint()
    {
        var newPoint = new GameObject("New Spawn Point");
        newPoint.AddComponent<SpawnPoint>();
    }
#endif

    private void Start()
    {
        data.pos = transform.position;
        data.enemyParent = enemyParent;
        data.SpawnPoint = this;
        EnemyManager.SpawnPoints.Add(data);
    }
}