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
 
    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Dictionary<Vector3, GameObject> towersByPos = new Dictionary<Vector3, GameObject>();
    public void SpawnTowerAt(Vector3 pos, Tower_SO dto)
    {
        GameObject tower = Instantiate(dto.minimapPrefab, transform);
        var selector = tower.GetComponent<TowerSelectPointController>();
        selector.towerDTO = dto;
        selector.RefreshText();
        selector.originTower = TowerSpawnManager._towersByPos[pos];
        tower.transform.position = _WorldPosToMinimapPos(pos);
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
}
