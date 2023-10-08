using System;
using System.Collections;
using System.Collections.Generic;
using Project.Towers.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

#if UNITY_EDITOR
[CustomEditor(typeof(Minimap))]
class MinimapEditor : Editor
{
    private Minimap m => (Minimap)target;
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Prepare Map Model"))
        {
            m.PrepareMapModel();
        }
        if (GUILayout.Button("Add mask to all"))
        {
            m.AddAllMasks();
        }

        string colEnabled = !m.CollidersEnabled ? "Enable" : "Disable";
        if (GUILayout.Button($"{colEnabled} Colliders"))
        {
            m.ToggleColliders();
        }
        base.OnInspectorGUI();
    }
}
#endif
public class Minimap : MonoBehaviour
{
    public static Minimap instance;

    public bool CollidersEnabled = false;
    public bool SetAsTrigger = true;
    [SerializeField] private Transform zeroPoint;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private Transform mapModelParent;
    [SerializeField] private Transform playerArrow;
    [SerializeField] private Transform baseCenter;

    public static Transform Zero => instance.zeroPoint;

    public GameObject VisibleCenterCollider;
    public static bool ColliderWasCenter(Collider col)
    {
        return col.gameObject == instance.VisibleCenterCollider;
    }
    private GameObject child;
    private Vector3 startZeroPos;
    private void Awake()
    {
        instance = this;
        child = transform.GetChild(0).gameObject;
        SpawnAllExisting();
        child.SetActive(false);
        startZeroPos = zeroPoint.localPosition;

    }

    public static float scale => instance._scale;
    private float _scale => mapModelParent.localScale.x;

    public Dictionary<BasicEnemy, Transform> enemies = new Dictionary<BasicEnemy, Transform>();

    public Vector2 xBounds = new Vector2(-4f, 4f);
    public Vector2 yBounds = new Vector2(-4f, 4f);
    // Update is called once per frame
    public Vector3 zeroLocal;
    void Update()
    {
        UpdateAllEnemyPositions();
        UpdatePlayerPosition();
        zeroLocal = zeroPoint.localPosition;
    }
    

    #region EnemyFunctions
    public static void SpawnHead(BasicEnemy enemy)
    {
        if (instance == null) return;
        GameObject head = Instantiate(enemy.headPrefab, Zero);
        instance.enemies.Add(enemy, head.transform);
        head.transform.localPosition = enemy.transform.position * scale;
    }

    public static void RemoveHead(BasicEnemy enemy)
    {
        if (instance == null) return;
        if (instance.enemies.ContainsKey(enemy))
        {
            Destroy(instance.enemies[enemy].gameObject);
            instance.enemies.Remove(enemy);
        }
    }
    void UpdateAllEnemyPositions()
    {
        foreach (var e in enemies)
        {
            var enemy = e.Key;
            var head = e.Value;
            try
            {
                head.localPosition = enemy.transform.position * Minimap.scale;
            }
            catch (MissingReferenceException exception)
            {
                continue;
            }
            
            Vector3 rot = head.localEulerAngles;
            rot.y = enemy.transform.localEulerAngles.y;
            head.localEulerAngles = rot;
        }
    }

    void UpdatePlayerPosition()
    {
        playerArrow.localPosition = InventoryManager.player.position * scale;
    }
    
    #endregion


    #region EditorFunctions
    #if UNITY_EDITOR
    public void PrepareMapModel()
    {
        Transform baseGrid = mapModelParent.Find("BaseGrid");
        foreach (Transform t in baseGrid)
        {
            var mrs = t.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var mr in mrs)
            {
                var masker = mr.GetComponent<MaskObject>();
                if (masker == null)
                    masker = mr.gameObject.AddComponent<MaskObject>();
                masker.mr = mr;

            }
            print($"Found {t.gameObject.name}");
            continue;
            if (t.GetComponent<BoxCollider>() == null)
            {
                var bc = t.gameObject.AddComponent<BoxCollider>();
                bc.size = new Vector3(100f, 1f, 100f);
                bc.center = Vector3.zero;
                bc.isTrigger = true;
            }

            var culler = t.GetComponent<MinimapCuller>();
            if (culler == null)
            {
                culler = t.gameObject.AddComponent<MinimapCuller>();
            }
            culler.Init();
        }
        print($"Map prepared :)");
    }
#endif
    public void AddAllMasks()
    {
        var mrs = GetComponentsInChildren<MeshRenderer>(true);
        foreach (var mr in mrs)
        {
            if (mr.gameObject.name == "Base")
                continue;
            AddMaskTo(mr);
        }
    }

    void AddMaskTo(MeshRenderer mr)
    {
        var mask = mr.GetComponent<MaskObject>();
        if (mask == null)
        {
            mask = mr.gameObject.AddComponent<MaskObject>();
            mask.mr = mr;
        }
    }

    public void ToggleColliders()
    {
        var cols = GetComponentsInChildren<Collider>(true);
        foreach (var col in cols)
        {
            if (col.gameObject.name == "Base")
                continue;
            if (col.GetComponent<XRBaseInteractable>() != null)
                continue;
            if (col is MeshCollider meshCollider)
            {
                meshCollider.convex = true;
            }
            col.enabled = !CollidersEnabled;
            col.isTrigger = SetAsTrigger;
        }

        CollidersEnabled = !CollidersEnabled;

    }
    #endregion

    #region TowerFunctions
    void SpawnAllExisting()
    {
        var towers = TowerSpawnManager.Instance.GetComponentsInChildren<Tower>();
        foreach (var tower in towers)
        {
            Vector3 pos = tower.transform.position;
            TowerSpawnManager._towersByPos.Add(pos, tower);
            SpawnTowerAt(pos, tower.dto);
        }
    }

    private Dictionary<Vector3, GameObject> towersByPos = new Dictionary<Vector3, GameObject>();

    public static void SpawnTower(Vector3 pos, Tower_SO dto)
    {
        if (instance == null) return;
        if (instance.towersByPos.ContainsKey(pos)) return;
        
        instance.SpawnTowerAt(pos, dto);
    }
    public void SpawnTowerAt(Vector3 pos, Tower_SO dto)
    {
        GameObject tower = Instantiate(dto.minimapPrefab, zeroPoint.transform);
        var selector = tower.GetComponent<TowerSelectPointController>();
        foreach (var mr in tower.GetComponentsInChildren<MeshRenderer>())
            AddMaskTo(mr);
        selector.towerDTO = dto;
        selector.RefreshText();
        selector.originTower = TowerSpawnManager._towersByPos[pos];
        //tower.transform.position = _WorldPosToMinimapPos(pos);
        tower.transform.localPosition = pos * _scale;
        //tower.transform.rotation = zeroPoint.rotation;
        towersByPos.Add(pos, tower);
    }

    public static void DestroyTower(Vector3 pos)
    {
        if (instance)
            instance.DestroyTowerAt(pos);
    }
    public void DestroyTowerAt(Vector3 pos)
    {
        if (towersByPos.ContainsKey(pos))
        {
            Destroy(towersByPos[pos]);
            towersByPos.Remove(pos);
        }
        else
        {
            //print($"No tower at {pos}");
        }
    }
    #endregion
    #region HelperFunctions

    private GrabRepositioner _repositioner;
    public void ResetMinimap()
    {
        if (_repositioner == null)
            _repositioner = GetComponentInChildren<GrabRepositioner>();
        _repositioner.Reset();
        CenterPlayer();
        //zeroPoint.localPosition = startZeroPos;
    }

    public static void CenterPlayer()
    {
        if (instance == null) return;

        Vector3 dir = instance.baseCenter.position - instance.playerArrow.position;
        dir.y = 0f;
        Zero.Translate(dir);
    }
    private Vector3 _WorldPosToMinimapPos(Vector3 pos)
    {
        pos *= _scale;
        pos = zeroPoint.transform.position + pos;
        return pos;
    }

    public static void SetActive(bool active)
    {
        if (instance == null) return;
        //if (instance.child == null) Debug.LogError($"No child on minimap, set on?{active}");
        if (instance.child == null) return;
        instance.child.SetActive(active);
    }

    public static bool IsActive()
    {
        return instance.child.activeInHierarchy;
    }
    #endregion
}
