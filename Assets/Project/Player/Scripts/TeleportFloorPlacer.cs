using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR
[CustomEditor(typeof(TeleportFloorPlacer))]
class TeleportPlacerEditor : Editor
{
    private TeleportFloorPlacer t => ((TeleportFloorPlacer)target);
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Spawn Floor"))
        {
            t.SpawnFloor();
        }
        base.OnInspectorGUI();
    }
}

#endif
public class TeleportFloorPlacer : MonoBehaviour
{
    public GameObject floorPrefab;
    public int distanceFromCenter = 7;

    public Transform player;

    public float refreshRate = 0.8f;

    public LayerMask ignore;
    public static TeleportFloorPlacer instance;
    IEnumerator _refresher;
    // Start is called before the first frame update
    void Start()
    {
        if (player == null && InventoryManager.instance != null)
            player = InventoryManager.instance.playerTransform;
        StartCoroutine(_RefreshTeleportHeightsLoop());
        instance = this;
    }

    public static void ManualRefresh()
    {
        if (instance == null) return;
        instance._RepositionSquares();
    }

    IEnumerator _RefreshTeleportHeightsLoop()
    {
        while (true)
        {
            _RepositionSquares();
            yield return new WaitForSeconds(refreshRate);
        }
    }

    private Vector3 _lastCenter = Vector3.negativeInfinity;
    Vector3 _center;
    void _RepositionSquares()
    {
        _center = player.transform.position;
        _center = Vector3Int.RoundToInt(_center);
        if (_center == _lastCenter)
            return;
        _lastCenter = _center;
        transform.position = _center;
        for (int i = distanceFromCenter * -1; i < distanceFromCenter; i++)
        {
            for (int j = distanceFromCenter * -1; j < distanceFromCenter; j++)
            {
                _RepositionSquare(i, j);
            }
        }
    }

    private Dictionary<Vector2, Transform> tiles = new Dictionary<Vector2, Transform>();
    void _RepositionSquare(int i, int j)
    {
        RaycastHit hit;
        Vector3 pos = _center;
        pos.x += i;
        pos.z += j;
        pos.y += .5f;
        Transform t = GetTile(i, j);
        if (Physics.Raycast(pos + new Vector3(0f, 100f, 0f), Vector3.down, out hit, Mathf.Infinity, ignore, QueryTriggerInteraction.Ignore))
        {
            pos.y = hit.point.y;
            if (hit.collider.gameObject.layer != 7)
            {
                pos.y = -1000f;
            }

            t.rotation = hit.transform.rotation;
        }
        else
        {
            pos.y = -1000f;
            t.rotation = quaternion.identity;
            
        }

        
        t.position = pos;
    }

    Transform GetTile(int i, int j)
    {
        Vector2 posInt = new Vector2(i, j);
        if (tiles.ContainsKey(posInt))
            return tiles[posInt];
        else
        {
            tiles[posInt] = transform.Find($"{i}, {j}");
            return tiles[posInt];
        }
    }

    [SerializeField]
    private Dictionary<Vector2Int, float> heightsByPos = new Dictionary<Vector2Int, float>();

    public void SpawnFloor()
    {
        transform.DestroyChildrenImmediate();
        _lastCenter = Vector3.negativeInfinity;
        Vector3 root = transform.position;
        tiles = new Dictionary<Vector2, Transform>();
        for (int i = distanceFromCenter * -1; i < distanceFromCenter; i++)
        {
            for (int j = distanceFromCenter * -1; j < distanceFromCenter; j++)
            {
                Vector3 pos = new Vector3(root.x + i, root.y, root.z + j);
                GameObject floor = Instantiate(floorPrefab, transform);
                floor.name = $"{i}, {j}";
                floor.transform.position = pos;
            }
        }
        _RepositionSquares();
    }
}
