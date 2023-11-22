using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR

using UnityEditor;
#endif
using UnityEngine;

public class SpawnPoint : PathPoint
{
    [SerializeField] private SpawnPointData data;
    public Transform enemyParent;
    public SpawnPointData GetData() => data;
#if UNITY_EDITOR

    protected override void OnDrawGizmos()
    {
        var position = transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(position, .5f);
        if (data)
        {
            /*
            try
            {
                var result = int.Parse(new string(data.name.Reverse()
                .TakeWhile(char.IsDigit)
                .Reverse()
                .ToArray()));
                Handles.Label(transform.position + Vector3.up * 2, result.ToString());
            }
            catch { }*/
            
        }

        var next = nextPoint;
        while (next)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(position, next.transform.position);

            position = next.transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(position, .5f);

            next = next.nextPoint;
        }
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
            if(selected.TryGetComponent(out PathPoint pathPoint) == false) continue;

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
}