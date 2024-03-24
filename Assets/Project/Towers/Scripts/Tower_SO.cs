using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Tower", fileName = "New Tower")]
[System.Serializable]
public class Tower_SO : ScriptableObject
{
    public bool IsUnlocked = true;
    [Header("Prefabs")]
    public GameObject ghostObject;
    public Tower towerPrefab;
    public GameObject iconPrefab;
    public GameObject minimapPrefab;
    
    [Header("Base Data")]
    public new string name;
    public string description;
    public int cost;
    public int maxHeath;

    [SerializeField] private List<TowerUpgrade> upgrades = new();

    public List<TowerUpgrade> GetUpgrades() => upgrades;
}

[Serializable]
public struct TowerUpgrade
{
    public Tower_SO upgrade;
    public int cost;
}