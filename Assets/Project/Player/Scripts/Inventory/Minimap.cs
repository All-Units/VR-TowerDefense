using System;
using System.Collections;
using System.Collections.Generic;
using Project.Towers.Scripts;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public static Minimap instance;

    [SerializeField] private Transform zeroPoint;
    [SerializeField] private GameObject towerPrefab;

    public static Transform Zero => instance.zeroPoint;

    private GameObject child;
    private void Awake()
    {
        instance = this;
        child = transform.GetChild(0).gameObject;
        child.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Dictionary<Vector3, GameObject> towersByPos = new Dictionary<Vector3, GameObject>();
    public void SpawnTowerAt(Vector3 pos, Tower_SO dto)
    {
        GameObject tower = Instantiate(dto.minimapPrefab, zeroPoint.transform);
        var selector = tower.GetComponent<TowerSelectPointController>();
        selector.towerDTO = dto;
        selector.RefreshText();
        selector.originTower = TowerSpawnManager._towersByPos[pos];
        //tower.transform.position = _WorldPosToMinimapPos(pos);
        tower.transform.localPosition = pos * 0.02f;
        //tower.transform.rotation = zeroPoint.rotation;
        towersByPos.Add(pos, tower);
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
            print($"No tower at {pos}");
        }
    }

    private Vector3 _WorldPosToMinimapPos(Vector3 pos)
    {
        pos *= 0.02f;
        pos = zeroPoint.transform.position + pos;
        return pos;
    }

    public static void SetActive(bool active)
    {
        if (instance == null) return;
        if (instance.child == null) Debug.LogError($"No child on minimap, set on?{active}");
        if (instance.child == null) return;
        instance.child.SetActive(active);
    }

    public static bool IsActive()
    {
        return instance.child.activeInHierarchy;
    }
}
